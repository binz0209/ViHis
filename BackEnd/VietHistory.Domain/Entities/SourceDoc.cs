using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VietHistory.Domain.Entities;

/// <summary>
/// Domain entity representing a source document (PDF, etc.)
/// </summary>
public sealed class SourceDoc
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string Title { get; set; } = default!;
    public string? Author { get; set; }
    public int? Year { get; set; }
    public string FileName { get; set; } = default!;
    public int Pages { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

