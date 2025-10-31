using System.Net.Http;
using VietHistory.Infrastructure.Services.Gemini;

namespace VietHistory.Infrastructure.Services
{
    /// <summary>
    /// Placeholder QuizGenerationService for DI wiring. Replace with real Gemini-based generation as needed.
    /// </summary>
    public class QuizGenerationService
    {
        private readonly HttpClient _http;
        private readonly GeminiOptions _opt;

        public QuizGenerationService(HttpClient http, GeminiOptions opt)
        {
            _http = http;
            _opt = opt;
        }
    }
}


