namespace VietHistory.Application.DTOs;

public record QuizDto(string Id, string CreatorId, string Topic, int MultipleChoiceCount, int EssayCount, List<QuizQuestionDto> Questions);

