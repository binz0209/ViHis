using MongoDB.Bson.Serialization.Attributes;
using VietHistory.Domain.Common;

namespace VietHistory.Domain.Entities;

public class ChatMessage : BaseEntity
{
    [BsonElement("text")] public string Text { get; set; } = string.Empty;
    [BsonElement("sender")] public string Sender { get; set; } = string.Empty; // "user" or "assistant"
    [BsonElement("chatId")] public string ChatId { get; set; } = string.Empty;
}

