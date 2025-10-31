using MongoDB.Bson.Serialization.Attributes;

namespace VietHistory.Domain.Entities;

public class EmbeddedQuizQuestion
{
    [BsonElement("id")] public string Id { get; set; } = string.Empty; // Unique ID for this question in the quiz
    [BsonElement("type")] public string Type { get; set; } = "multipleChoice"; // "multipleChoice" or "essay"
    [BsonElement("question")] public string Question { get; set; } = string.Empty;
    [BsonElement("options")] public List<string> Options { get; set; } = new(); // For multiple choice
    [BsonElement("correctAnswerIndex")] public int? CorrectAnswerIndex { get; set; } // For multiple choice
}

