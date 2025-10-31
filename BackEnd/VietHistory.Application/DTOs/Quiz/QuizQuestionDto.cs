namespace VietHistory.Application.DTOs;

public record QuizQuestionDto(string Id, string Type, string Question, List<string>? Options, int? CorrectAnswerIndex);

