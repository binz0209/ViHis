using MongoDB.Bson.Serialization.Attributes;
using VietHistory.Domain.Common;

namespace VietHistory.Domain.Entities;

public class AppRole : BaseEntity
{
    [BsonElement("name")] public string Name { get; set; } = string.Empty;
    [BsonElement("permissions")] public List<string> Permissions { get; set; } = new();
}

