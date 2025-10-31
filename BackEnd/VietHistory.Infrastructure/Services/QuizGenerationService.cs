using System.Net.Http;
using MongoDB.Bson;
using VietHistory.Domain.Entities;
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

        public Task<List<EmbeddedQuizQuestion>> GenerateQuizQuestionsAsync(
            string topic,
            int multipleChoiceCount,
            int essayCount)
        {
            var questions = new List<EmbeddedQuizQuestion>();

            for (int i = 0; i < multipleChoiceCount; i++)
            {
                questions.Add(new EmbeddedQuizQuestion
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Type = "multipleChoice",
                    Question = $"[{topic}] MCQ #{i + 1}: Choose the correct statement.",
                    Options = new List<string> { "Option A", "Option B", "Option C", "Option D" },
                    CorrectAnswerIndex = 0
                });
            }

            for (int i = 0; i < essayCount; i++)
            {
                questions.Add(new EmbeddedQuizQuestion
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Type = "essay",
                    Question = $"[{topic}] Essay #{i + 1}: Discuss briefly.",
                    Options = new List<string>(),
                    CorrectAnswerIndex = null
                });
            }

            return Task.FromResult(questions);
        }
    }
}


