using MongoDB.Driver;
using VietHistory.Domain.Entities;
using VietHistory.Domain.Repositories;
using VietHistory.Infrastructure.Mongo;

namespace VietHistory.Infrastructure.Mongo.Repositories;

/// <summary>
/// Implementation of ISourceRepository (Port from Domain layer)
/// </summary>
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

