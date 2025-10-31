namespace VietHistory.Application.DTOs;

public record AiAskRequest(
    string Question, 
    string? Language, 
    int MaxContext = 5, 
    string? BoxId = null, 
    List<ChatMessageDto>? ChatHistory = null);

