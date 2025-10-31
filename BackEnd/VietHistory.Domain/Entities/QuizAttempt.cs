using System;
using MongoDB.Bson.Serialization.Attributes;
using VietHistory.Domain.Common;

namespace VietHistory.Domain.Entities;

public class QuizAttempt : BaseEntity
{
    [BsonElement("quizId")] public string QuizId { get; set; } = string.Empty;
    [BsonElement("userId")] public string UserId { get; set; } = string.Empty;
    [BsonElement("answers")] public Dictionary<string, string> Answers { get; set; } = new(); // questionId -> answer
    [BsonElement("score")] public int? Score { get; set; } // Calculated score
    [BsonElement("totalQuestions")] public int TotalQuestions { get; set; } = 0;
    [BsonElement("completedAt")] public DateTime? CompletedAt { get; set; }
}

