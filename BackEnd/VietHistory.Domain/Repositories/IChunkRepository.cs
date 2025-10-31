using System.Collections.Generic;
using MongoDB.Driver;
using VietHistory.Domain.Entities;

namespace VietHistory.Domain.Repositories;

/// <summary>
/// Port (Interface) for Chunk Repository - defined in Domain layer
/// Implementation will be in Infrastructure layer
/// </summary>
public interface IChunkRepository
{
    IMongoCollection<ChunkDoc> Collection { get; }
    Task InsertManyAsync(IEnumerable<ChunkDoc> docs, CancellationToken ct = default);
    Task CreateIndexesAsync(CancellationToken ct = default);
    Task InsertAsync(ChunkDoc doc, CancellationToken ct = default);
}

