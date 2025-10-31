namespace VietHistory.Application.DTOs;

public record SubmitQuizRequest(string QuizId, Dictionary<string, string> Answers);

