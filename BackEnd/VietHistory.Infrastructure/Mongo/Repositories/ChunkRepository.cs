using System.Collections.Generic;
using MongoDB.Driver;
using VietHistory.Domain.Entities;
using VietHistory.Domain.Repositories;

namespace VietHistory.Infrastructure.Mongo.Repositories;

/// <summary>
/// Implementation of IChunkRepository (Port from Domain layer)
/// </summary>
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

