using VietHistory.Application.DTOs;

namespace VietHistory.Application.Services;

public interface IPeopleService
{
    Task<PersonDto> CreateAsync(CreatePersonRequest req);
    Task<IReadOnlyList<PersonDto>> ListAsync(string? q = null, int skip = 0, int take = 50);
}

public interface IEventsService
{
    Task<EventDto> CreateAsync(CreateEventRequest req);
    Task<IReadOnlyList<EventDto>> ListAsync(int? month = null, int? day = null, string? q = null, int skip = 0, int take = 50);
}

public interface IAIStudyService
{
    Task<AiAnswer> AskAsync(AiAskRequest req);
}

public interface IQuizService
{
    Task<QuizDto> CreateQuizAsync(string creatorId, CreateQuizRequest req);
    Task<QuizDto> GetQuizAsync(string quizId);
    Task<QuizAttemptDto> SubmitQuizAsync(string userId, SubmitQuizRequest req);
    Task<IReadOnlyList<QuizDto>> GetUserQuizzesAsync(string userId);
    Task<QuizAttemptDto?> GetUserQuizAttemptAsync(string quizId, string userId);
}
