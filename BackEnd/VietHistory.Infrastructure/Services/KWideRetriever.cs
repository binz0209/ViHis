using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using VietHistory.Application.DTOs.Ingest;
using VietHistory.Infrastructure.Mongo;

namespace VietHistory.Infrastructure.Services.AI
{
    /// <summary>
    /// K-Wide retrieval với caching và truy vấn song song.
    /// Mục tiêu: tốc độ cao ngay cả khi lấy maxContext = 20.
    /// </summary>
    public sealed class KWideRetriever
    {
        private readonly IMongoContext _ctx;
        private static readonly MemoryCache Cache = new(new MemoryCacheOptions());
        private const int CacheMinutes = 15;

        public KWideRetriever(IMongoContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<List<ChunkDoc>> GetKWideChunksAsync(
            string question,
            int k = 20,
            int windowSize = 1,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(question)) return new();

            // ✅ Cache theo câu hỏi (dưới dạng key hash)
            var key = $"kwide:{question.Trim().ToLowerInvariant()}:{k}:{windowSize}";
            if (Cache.TryGetValue(key, out List<ChunkDoc>? cached))
                return cached!;

            // 1️⃣ Query top-k chunk
            var filter = Builders<ChunkDoc>.Filter.Text(question);
            var sort = Builders<ChunkDoc>.Sort.MetaTextScore("score");

            var projection = Builders<ChunkDoc>.Projection
                .Include(x => x.SourceId)
                .Include(x => x.ChunkIndex)
                .Include(x => x.Content)
                .Include(x => x.PageFrom)
                .Include(x => x.PageTo);

            var baseChunks = await _ctx.Chunks
                .Find(filter)
                .Project<ChunkDoc>(projection)
                .Sort(sort)
                .Limit(k)
                .ToListAsync(ct);


            // Fallback regex (chỉ khi không có index)
            if (baseChunks.Count == 0)
            {
                var regex = new MongoDB.Bson.BsonRegularExpression(System.Text.RegularExpressions.Regex.Escape(question), "i");
                baseChunks = await _ctx.Chunks
                    .Find(Builders<ChunkDoc>.Filter.Regex(x => x.Content, regex))
                    .Limit(k)
                    .ToListAsync(ct);
            }

            if (baseChunks.Count == 0)
                return baseChunks;

            // 2️⃣ Lấy danh sách sourceId (song song)
            var srcIds = baseChunks.Select(c => c.SourceId).Distinct().ToList();
            var bag = new ConcurrentBag<ChunkDoc>();

            await Parallel.ForEachAsync(srcIds, ct, async (srcId, token) =>
            {
                var allChunks = await _ctx.Chunks.Find(c => c.SourceId == srcId)
                    .Project(c => new ChunkDoc
                    {
                        Id = c.Id,
                        SourceId = c.SourceId,
                        ChunkIndex = c.ChunkIndex,
                        Content = c.Content,
                        PageFrom = c.PageFrom,
                        PageTo = c.PageTo
                    })
                    .SortBy(c => c.ChunkIndex)
                    .ToListAsync(token);

                foreach (var c in baseChunks.Where(x => x.SourceId == srcId))
                {
                    var start = Math.Max(0, c.ChunkIndex - windowSize);
                    var end = Math.Min(allChunks.Count - 1, c.ChunkIndex + windowSize);
                    foreach (var e in allChunks.Where(x => x.ChunkIndex >= start && x.ChunkIndex <= end))
                        bag.Add(e);
                }
            });

            // 3️⃣ Distinct + sort
            var result = bag
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .OrderBy(c => c.SourceId)
                .ThenBy(c => c.ChunkIndex)
                .Take(k * (windowSize * 2 + 1))
                .ToList();

            // ✅ Cache 15 phút
            Cache.Set(key, result, TimeSpan.FromMinutes(CacheMinutes));

            return result;
        }
    }
}
