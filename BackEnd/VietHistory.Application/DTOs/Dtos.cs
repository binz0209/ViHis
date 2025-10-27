namespace VietHistory.Application.DTOs;

public record PersonDto(string Id, string FullName, int? BornYear, int? DiedYear, string? Summary);
public record EventDto(string Id, string Title, int? Year, int? Month, int? Day, string? Summary);
public record CreatePersonRequest(string FullName, int? BornYear, int? DiedYear, string? Summary);
public record CreateEventRequest(string Title, int? Year, int? Month, int? Day, string? Summary);
public record ChatMessageDto(string Id, string Text, string Sender, DateTime Timestamp);
public record AiAskRequest(string Question, string? Language, int MaxContext = 5, string? BoxId = null, List<ChatMessageDto>? ChatHistory = null);
public record AiAnswer(string Answer, string Model, double? CostUsd = null);

// Quiz DTOs
public record QuizQuestionDto(string Id, string Type, string Question, List<string>? Options, int? CorrectAnswerIndex);
public record QuizDto(string Id, string CreatorId, string Topic, int MultipleChoiceCount, int EssayCount, List<QuizQuestionDto> Questions);
public record CreateQuizRequest(string Topic, int MultipleChoiceCount, int EssayCount);
public record SubmitQuizRequest(string QuizId, Dictionary<string, string> Answers);
public record QuizAnswerResult(string QuestionId, bool IsCorrect, string? UserAnswer, string? CorrectAnswer);
public record QuizAttemptDto(string Id, string QuizId, string UserId, int? Score, int TotalQuestions, DateTime? CompletedAt, List<QuizAnswerResult>? AnswerResults = null);
