using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace VietHistory.Domain.Common;

public abstract class BaseEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("slug")]
    public string Slug { get; set; } = string.Empty;

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();
}
