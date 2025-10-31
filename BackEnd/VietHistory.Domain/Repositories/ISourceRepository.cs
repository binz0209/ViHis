using MongoDB.Driver;
using VietHistory.Domain.Entities;

namespace VietHistory.Domain.Repositories;

/// <summary>
/// Port (Interface) for Source Repository - defined in Domain layer
/// Implementation will be in Infrastructure layer
/// </summary>
public interface ISourceRepository
{
    IMongoCollection<SourceDoc> Collection { get; }
    Task<string> InsertAsync(SourceDoc doc, CancellationToken ct = default);
}

