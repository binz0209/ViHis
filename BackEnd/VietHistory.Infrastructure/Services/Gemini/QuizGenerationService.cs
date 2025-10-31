using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VietHistory.Domain.Entities;

namespace VietHistory.Infrastructure.Services.Gemini;

public class QuizGenerationService
{
    private static readonly JsonSerializerOptions JsonOpt = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private readonly GeminiOptions _opt;

    public QuizGenerationService(HttpClient http, GeminiOptions opt)
    {
        _http = http;
        _opt = opt;
        _http.Timeout = TimeSpan.FromSeconds(120); // Longer timeout for quiz generation
    }

    public async Task<List<EmbeddedQuizQuestion>> GenerateQuizQuestionsAsync(
        string topic,
        int multipleChoiceCount,
        int essayCount,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_opt.ApiKey))
            throw new InvalidOperationException("❌ Missing Gemini API key. Set GEMINI_API_KEY or appsettings:Gemini:ApiKey.");

        if (string.IsNullOrWhiteSpace(_opt.Model))
            throw new InvalidOperationException("❌ Missing Gemini model. Set appsettings:Gemini:Model.");

        var questions = new List<EmbeddedQuizQuestion>();

        // Generate multiple choice questions
        if (multipleChoiceCount > 0)
        {
            var mcQuestions = await GenerateMultipleChoiceQuestionsAsync(topic, multipleChoiceCount, ct);
            questions.AddRange(mcQuestions);
        }

        // Generate essay questions
        if (essayCount > 0)
        {
            var essayQuestions = await GenerateEssayQuestionsAsync(topic, essayCount, ct);
            questions.AddRange(essayQuestions);
        }

        return questions;
    }

    private async Task<List<EmbeddedQuizQuestion>> GenerateMultipleChoiceQuestionsAsync(
        string topic,
        int count,
        CancellationToken ct)
    {
        var prompt = $@"Tạo {count} câu hỏi trắc nghiệm về chủ đề '{topic}' trong lịch sử Việt Nam.
Mỗi câu hỏi phải có:
- Câu hỏi rõ ràng, liên quan đến chủ đề
- 4 lựa chọn (A, B, C, D)
- Đáp án đúng được chỉ định bằng index (0, 1, 2, hoặc 3)

Trả lời theo định dạng JSON sau (không có markdown code block):
{{
  ""questions"": [
    {{
      ""question"": ""Câu hỏi 1?"",
      ""options"": [""Lựa chọn A"", ""Lựa chọn B"", ""Lựa chọn C"", ""Lựa chọn D""],
      ""correctAnswerIndex"": 0
    }}
  ]
}}";

        var response = await CallGeminiAsync(prompt, ct);
        return ParseMultipleChoiceResponse(response, count, topic);
    }

    private async Task<List<EmbeddedQuizQuestion>> GenerateEssayQuestionsAsync(
        string topic,
        int count,
        CancellationToken ct)
    {
        var prompt = $@"Tạo {count} câu hỏi tự luận về chủ đề '{topic}' trong lịch sử Việt Nam.
Mỗi câu hỏi phải:
- Yêu cầu học sinh trình bày, phân tích hoặc giải thích
- Liên quan trực tiếp đến chủ đề
- Phù hợp với học sinh phổ thông

Trả lời theo định dạng JSON sau (không có markdown code block):
{{
  ""questions"": [
    {{
      ""question"": ""Câu hỏi tự luận 1?""
    }}
  ]
}}";

        var response = await CallGeminiAsync(prompt, ct);
        return ParseEssayResponse(response, count, topic);
    }

    private async Task<string> CallGeminiAsync(string prompt, CancellationToken ct)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_opt.Model}:generateContent?key={_opt.ApiKey}";
        
        var body = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = prompt } }
                }
            },
            generationConfig = new
            {
                temperature = 0.7,
                maxOutputTokens = 4000
            }
        };

        using var httpContent = new StringContent(
            JsonSerializer.Serialize(body, JsonOpt),
            Encoding.UTF8,
            "application/json");

        using var resp = await _http.PostAsync(url, httpContent, ct);
        var respText = await resp.Content.ReadAsStringAsync(ct);
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(respText);
        var root = doc.RootElement;

        // Extract text from response
        if (root.TryGetProperty("candidates", out var candidates) &&
            candidates.GetArrayLength() > 0)
        {
            var candidate = candidates[0];
            if (candidate.TryGetProperty("content", out var content) &&
                content.TryGetProperty("parts", out var parts))
            {
                var partsArray = parts.EnumerateArray();
                foreach (var part in partsArray)
                {
                    if (part.TryGetProperty("text", out var text))
                    {
                        return text.GetString() ?? "";
                    }
                }
            }
        }

        throw new InvalidOperationException("Failed to extract response from Gemini API");
    }

    private List<EmbeddedQuizQuestion> ParseMultipleChoiceResponse(string response, int expectedCount, string topic)
    {
        var questions = new List<EmbeddedQuizQuestion>();
        
        try
        {
            // Try to parse JSON directly
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                using var doc = JsonDocument.Parse(json);
                
                if (doc.RootElement.TryGetProperty("questions", out var questionsArray))
                {
                    int index = 0;
                    foreach (var q in questionsArray.EnumerateArray())
                    {
                        var question = new EmbeddedQuizQuestion
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = "multipleChoice",
                            Question = q.TryGetProperty("question", out var qText) ? qText.GetString() ?? "" : "",
                            Options = new List<string>(),
                            CorrectAnswerIndex = null
                        };

                        if (q.TryGetProperty("options", out var options))
                        {
                            foreach (var opt in options.EnumerateArray())
                            {
                                question.Options.Add(opt.GetString() ?? "");
                            }
                        }

                        if (q.TryGetProperty("correctAnswerIndex", out var correctIdx))
                        {
                            question.CorrectAnswerIndex = correctIdx.GetInt32();
                        }

                        if (!string.IsNullOrWhiteSpace(question.Question) && question.Options.Count >= 4)
                        {
                            questions.Add(question);
                            index++;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error parsing multiple choice response: {ex.Message}");
        }

        // Fallback: Generate simple questions if parsing fails
        if (questions.Count < expectedCount)
        {
            for (int i = questions.Count; i < expectedCount; i++)
            {
                questions.Add(new EmbeddedQuizQuestion
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "multipleChoice",
                    Question = $"Câu hỏi {i + 1} về {topic}?",
                    Options = new List<string> { "Đáp án A", "Đáp án B", "Đáp án C", "Đáp án D" },
                    CorrectAnswerIndex = 0
                });
            }
        }

        return questions.Take(expectedCount).ToList();
    }

    private List<EmbeddedQuizQuestion> ParseEssayResponse(string response, int expectedCount, string topic)
    {
        var questions = new List<EmbeddedQuizQuestion>();

        try
        {
            // Try to parse JSON directly
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("questions", out var questionsArray))
                {
                    foreach (var q in questionsArray.EnumerateArray())
                    {
                        var question = new EmbeddedQuizQuestion
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = "essay",
                            Question = q.TryGetProperty("question", out var qText) ? qText.GetString() ?? "" : "",
                            Options = new List<string>(),
                            CorrectAnswerIndex = null
                        };

                        if (!string.IsNullOrWhiteSpace(question.Question))
                        {
                            questions.Add(question);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error parsing essay response: {ex.Message}");
        }

        // Fallback: Generate simple questions if parsing fails
        if (questions.Count < expectedCount)
        {
            for (int i = questions.Count; i < expectedCount; i++)
            {
                questions.Add(new EmbeddedQuizQuestion
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "essay",
                    Question = $"Trình bày về {topic} trong lịch sử Việt Nam?",
                    Options = new List<string>(),
                    CorrectAnswerIndex = null
                });
            }
        }

        return questions.Take(expectedCount).ToList();
    }
}

