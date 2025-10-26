using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VietHistory.Domain.Common;
using VietHistory.Application.DTOs.Ingest;

namespace VietHistory.Infrastructure.Mongo
{
    /// <summary>
    /// Generic repository cho MongoDB (Entity kế thừa BaseEntity)
    /// </summary>
    public class MongoRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly IMongoCollection<T> _col;

        public MongoRepository(IMongoDatabase db, string collectionName)
        {
            _col = db.GetCollection<T>(collectionName);
        }

        public MongoRepository(IMongoCollection<T> collection)
        {
            _col = collection;
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            return await _col.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<T>> GetAllAsync(int skip = 0, int take = 100)
        {
            return await _col.Find(_ => true).Skip(skip).Limit(take).ToListAsync();
        }

        public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, int skip = 0, int take = 50)
        {
            return await _col.Find(predicate).Skip(skip).Limit(take).ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _col.InsertOneAsync(entity);
            return entity;
        }

        public async Task AddManyAsync(IEnumerable<T> entities)
        {
            await _col.InsertManyAsync(entities);
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            var res = await _col.ReplaceOneAsync(x => x.Id == entity.Id, entity);
            return res.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var res = await _col.DeleteOneAsync(x => x.Id == id);
            return res.DeletedCount > 0;
        }

        public async Task<long> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _col.CountDocumentsAsync(predicate);
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            var count = await _col.CountDocumentsAsync(predicate, new CountOptions { Limit = 1 });
            return count > 0;
        }

        public IMongoCollection<T> Collection => _col;
    }

    // ===========================================================
    // ==  Repository chuyên biệt cho SourceDoc & ChunkDoc (AI) ==
    // ===========================================================

    public interface ISourceRepository
    {
        IMongoCollection<SourceDoc> Collection { get; }
        Task<string> InsertAsync(SourceDoc doc, CancellationToken ct = default);
    }

    public interface IChunkRepository
    {
        IMongoCollection<ChunkDoc> Collection { get; }
        Task InsertManyAsync(IEnumerable<ChunkDoc> docs, CancellationToken ct = default);
        Task CreateIndexesAsync(CancellationToken ct = default);
        Task InsertAsync(ChunkDoc doc, CancellationToken ct = default);

    }

    public sealed class SourceRepository : ISourceRepository
    {
        public IMongoCollection<SourceDoc> Collection { get; }

        public SourceRepository(MongoContext ctx)
        {
            Collection = ctx.Db.GetCollection<SourceDoc>("sources");
        }

        public async Task<string> InsertAsync(SourceDoc doc, CancellationToken ct = default)
        {
            await Collection.InsertOneAsync(doc, cancellationToken: ct);
            return doc.Id;
        }
    }

    public sealed class ChunkRepository : IChunkRepository
    {
        public IMongoCollection<ChunkDoc> Collection { get; }

        public ChunkRepository(MongoContext ctx)
        {
            Collection = ctx.Db.GetCollection<ChunkDoc>("chunks");
        }

        public async Task InsertManyAsync(IEnumerable<ChunkDoc> docs, CancellationToken ct = default)
        {
            await Collection.InsertManyAsync(docs, cancellationToken: ct);
        }

        public async Task CreateIndexesAsync(CancellationToken ct = default)
        {
            // Tạo index kép (SourceId + ChunkIndex)
            var pairIndex = Builders<ChunkDoc>.IndexKeys
                .Ascending(x => x.SourceId)
                .Ascending(x => x.ChunkIndex);

            await Collection.Indexes.CreateOneAsync(new CreateIndexModel<ChunkDoc>(pairIndex), cancellationToken: ct);

            // Tạo full-text index trên nội dung chunk
            var textIndex = Builders<ChunkDoc>.IndexKeys.Text(x => x.Content);
            await Collection.Indexes.CreateOneAsync(new CreateIndexModel<ChunkDoc>(textIndex), cancellationToken: ct);
        }
        public async Task InsertAsync(ChunkDoc doc, CancellationToken ct = default)
        {
            await Collection.InsertOneAsync(doc, cancellationToken: ct);
        }

    }
}
