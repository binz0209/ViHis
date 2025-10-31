namespace VietHistory.Application.DTOs;

public record CreateQuizRequest(string Topic, int MultipleChoiceCount, int EssayCount);

