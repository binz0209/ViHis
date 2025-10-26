using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using VietHistory.Application.DTOs.Ingest;
using VietHistory.Infrastructure.Mongo;
using VietHistory.Infrastructure.Services.Gemini;

namespace VietHistory.Infrastructure.Services.TextIngest
{
    public interface IFallbackAIngestor
    {
        Task<(IReadOnlyList<Chunk> Chunks, int TotalPages)> RunAsync(Stream pdfStream, string sourceId, ParserProfile? profile = null);
    }

    public sealed class FallbackAIngestor : IFallbackAIngestor
    {
        private readonly IPdfTextExtractor _extractor;
        private readonly IChunkRepository _chunkRepository;
        private readonly GeminiOptions _geminiOptions;

        public FallbackAIngestor(IPdfTextExtractor extractor, IChunkRepository chunkRepo, GeminiOptions gemini)
        {
            _extractor = extractor;
            _chunkRepository = chunkRepo;
            _geminiOptions = gemini;
        }

        public async Task<(IReadOnlyList<Chunk> Chunks, int TotalPages)> RunAsync(Stream pdfStream, string sourceId, ParserProfile? profile = null)
        {
            var pf = profile ?? new ParserProfile();

            // 1️⃣ Extract pages
            var pages = _extractor.ExtractPages(pdfStream)
                .Select(p => new PageText { PageNumber = p.PageNumber, Raw = TextNormalizer.CleanRaw(p.Raw) })
                .OrderBy(p => p.PageNumber)
                .ToList();

            // 2️⃣ Merge short pages
            for (int i = 0; i < pages.Count - 1;)
            {
                if (pages[i].Raw.Length < 400)
                {
                    pages[i + 1] = new PageText
                    {
                        PageNumber = pages[i].PageNumber,
                        Raw = pages[i].Raw + "\n" + pages[i + 1].Raw
                    };
                    pages.RemoveAt(i);
                }
                else i++;
            }

            // 3️⃣ Detect & clean headers/footers
            var (headers, footers) = HeaderFooterDetector.Detect(pages, 2, 2, 0.8);
            var cleaned = HeaderFooterDetector.RemoveHeadersFooters(pages, headers, footers)
                .Select(p => new PageText
                {
                    PageNumber = p.PageNumber,
                    Raw = Regex.Replace(p.Raw, @"(\[Trang\s+\d+\]\s*){2,}", "[Trang $1]", RegexOptions.IgnoreCase)
                })
                .ToList();

            // 4️⃣ Split to sentences (keep [Trang X])
            var sentencesWithPage = new List<(string sentence, int page)>();
            foreach (var p in cleaned)
            {
                var lines = p.Raw.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
                var tag = lines.Count > 0 && lines[0].StartsWith("[Trang ") ? lines[0] : $"[Trang {p.PageNumber}]";
                var body = string.Join("\n", lines.Where((_, i) => i > 0));

                var sents = SentenceTokenizer.SplitSentences(body, pf.Abbreviations);
                sentencesWithPage.Add((tag, p.PageNumber));
                foreach (var s in sents) sentencesWithPage.Add((s, p.PageNumber));
            }

            // 5️⃣ Pack chunks
            var chunks = new List<Chunk>();
            var buffer = new List<(string sentence, int page)>();
            int bufTok = 0;
            int idx = 0;

            foreach (var item in sentencesWithPage)
            {
                var t = ChunkPack.ApproxTokens(item.sentence);
                if (bufTok + t > pf.MinChunkTokens && buffer.Count > 0)
                {
                    chunks.Add(BuildChunk(idx++, buffer));
                    var back = new List<(string sentence, int page)>();
                    int backTok = 0;
                    for (int i = buffer.Count - 1; i >= 0 && backTok < pf.OverlapTokens; i--)
                    {
                        back.Insert(0, buffer[i]);
                        backTok += ChunkPack.ApproxTokens(buffer[i].sentence);
                    }
                    buffer = back;
                    bufTok = backTok;
                }
                buffer.Add(item);
                bufTok += t;
            }
            if (buffer.Count > 0) chunks.Add(BuildChunk(idx++, buffer));

            // 6️⃣ Tạo embedding + lưu Mongo song song
            await Parallel.ForEachAsync(chunks, async (chunk, token) =>
            {
                try
                {
                    var embedding = await GetEmbeddingAsync(chunk.Content, _geminiOptions.ApiKey);
                    var chunkDoc = new ChunkDoc
                    {
                        SourceId = sourceId,
                        ChunkIndex = chunk.ChunkIndex,
                        Content = chunk.Content,
                        PageFrom = chunk.PageFrom,
                        PageTo = chunk.PageTo,
                        ApproxTokens = chunk.ApproxTokens,
                        CreatedAt = DateTime.UtcNow,
                        Embedding = embedding
                    };

                    await _chunkRepository.InsertAsync(chunkDoc);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Lỗi embedding chunk {chunk.ChunkIndex}: {ex.Message}");
                }
            });

            return (chunks, pages.Count);
        }

        private static Chunk BuildChunk(int idx, List<(string sentence, int page)> buffer)
        {
            var pages = buffer.Select(x => x.page).ToList();
            var pFrom = pages.Min();
            var pTo = pages.Max();
            var text = string.Join(" ", buffer.Select(x => x.sentence));

            if (!text.StartsWith("[Trang "))
                text = $"[Trang {pFrom}] " + text;

            return new Chunk
            {
                ChunkIndex = idx,
                Content = text.Trim(),
                PageFrom = pFrom,
                PageTo = pTo,
                ApproxTokens = ChunkPack.ApproxTokens(text)
            };
        }

        private async Task<List<float>> GetEmbeddingAsync(string text, string apiKey)
        {
            using var client = new HttpClient();
            var body = new
            {
                model = "embedding-001", // Gemini embedding model
                content = new { parts = new[] { new { text } } }
            };

            var json = JsonSerializer.Serialize(body);
            var resp = await client.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/embedding-001:embedContent?key={apiKey}",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var str = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(str);
            var emb = doc.RootElement.GetProperty("embedding").GetProperty("values");
            return emb.EnumerateArray().Select(x => x.GetSingle()).ToList();
        }
    }
}
