using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VietHistory.Domain.Entities;

/// <summary>
/// Domain entity representing a chunk of text from a source document
/// </summary>
public sealed class ChunkDoc
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonRepresentation(BsonType.ObjectId)]
    public string SourceId { get; set; } = default!;

    public int ChunkIndex { get; set; }
    public string Content { get; set; } = default!;
    public int PageFrom { get; set; }
    public int PageTo { get; set; }
    public int ApproxTokens { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ✅ Thêm field vector
    [BsonElement("embedding")]
    public List<float>? Embedding { get; set; }
}

