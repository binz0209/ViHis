using System;
using MongoDB.Bson.Serialization.Attributes;
using VietHistory.Domain.Common;

namespace VietHistory.Domain.Entities;

public class ChatHistory : BaseEntity
{
    [BsonElement("machineId")] public string MachineId { get; set; } = string.Empty;
    [BsonElement("userId")] public string? UserId { get; set; }
    [BsonElement("name")] public string Name { get; set; } = "Chat";
    [BsonElement("lastMessageAt")] public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
    [BsonElement("messageIds")] public List<string> MessageIds { get; set; } = new();
}

