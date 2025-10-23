namespace VietHistory.Application.DTOs;

public record PersonDto(string Id, string FullName, int? BornYear, int? DiedYear, string? Summary);
public record EventDto(string Id, string Title, int? Year, int? Month, int? Day, string? Summary);
public record CreatePersonRequest(string FullName, int? BornYear, int? DiedYear, string? Summary);
public record CreateEventRequest(string Title, int? Year, int? Month, int? Day, string? Summary);
public record AiAskRequest(string Question, string? Language, int MaxContext = 5);
public record AiAnswer(string Answer, string Model, double? CostUsd = null);
