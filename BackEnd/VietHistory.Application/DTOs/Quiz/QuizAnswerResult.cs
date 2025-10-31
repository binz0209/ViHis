namespace VietHistory.Application.DTOs;

public record QuizAnswerResult(string QuestionId, bool IsCorrect, string? UserAnswer, string? CorrectAnswer);

