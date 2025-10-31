using MongoDB.Bson.Serialization.Attributes;
using VietHistory.Domain.Common;

namespace VietHistory.Domain.Entities;

public class Quiz : BaseEntity
{
    [BsonElement("creatorId")] public string CreatorId { get; set; } = string.Empty;
    [BsonElement("topic")] public string Topic { get; set; } = string.Empty;
    [BsonElement("multipleChoiceCount")] public int MultipleChoiceCount { get; set; } = 0;
    [BsonElement("essayCount")] public int EssayCount { get; set; } = 0;
    [BsonElement("questions")] public List<EmbeddedQuizQuestion> Questions { get; set; } = new();
}

