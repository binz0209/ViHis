using VietHistory.Application.DTOs;

namespace VietHistory.Application.Services;

public interface IAIStudyService
{
    Task<AiAnswer> AskAsync(AiAskRequest req);
}

