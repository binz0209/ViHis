using System;

namespace VietHistory.Application.DTOs;

public record QuizAttemptDto(string Id, string QuizId, string UserId, int? Score, int TotalQuestions, DateTime? CompletedAt, List<QuizAnswerResult>? AnswerResults = null);

