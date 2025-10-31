using VietHistory.Application.DTOs;

namespace VietHistory.Application.Services;

public interface IQuizService
{
    Task<QuizDto> CreateQuizAsync(string creatorId, CreateQuizRequest req);
    Task<QuizDto> GetQuizAsync(string quizId);
    Task<QuizAttemptDto> SubmitQuizAsync(string userId, SubmitQuizRequest req);
    Task<IReadOnlyList<QuizDto>> GetUserQuizzesAsync(string userId);
    Task<QuizAttemptDto?> GetUserQuizAttemptAsync(string quizId, string userId);
}

