using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VietHistory.Infrastructure.Services;

public class EmbeddingService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;

    private static readonly JsonSerializerOptions JsonOpt = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions SnakeCaseOpt = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public EmbeddingService(HttpClient http, string apiKey, string model = "text-embedding-004")
    {
        _http = http;
        _apiKey = apiKey;
        _model = model; // Gemini embedding model: text-embedding-004 hoặc embedding-001
        _http.Timeout = TimeSpan.FromSeconds(60);
    }

    public async Task<List<float>> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be null or empty", nameof(text));

        try
        {
            // Gemini Embedding API endpoint
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:embedContent?key={_apiKey}";
            
            var payload = new
            {
                task_type = "RETRIEVAL_DOCUMENT",
                content = new { parts = new[] { new { text } } }
            };

            var json = JsonSerializer.Serialize(payload, SnakeCaseOpt);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _http.PostAsync(url, content, ct);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(responseJson);

            if (doc.RootElement.TryGetProperty("embedding", out var embedding) &&
                embedding.TryGetProperty("values", out var values))
            {
                var floats = new List<float>();
                foreach (var element in values.EnumerateArray())
                {
                    if (element.TryGetSingle(out var val))
                        floats.Add(val);
                }
                return floats;
            }

            throw new InvalidOperationException("Failed to parse embedding from API response");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to generate embedding: {ex.Message}", ex);
        }
    }

    public async Task<List<float>> GenerateQueryEmbeddingAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text cannot be null or empty", nameof(text));

        try
        {
            // Gemini Embedding API endpoint
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:embedContent?key={_apiKey}";
            
            var payload = new
            {
                task_type = "RETRIEVAL_QUERY",
                content = new { parts = new[] { new { text } } }
            };

            var json = JsonSerializer.Serialize(payload, SnakeCaseOpt);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _http.PostAsync(url, content, ct);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(responseJson);

            if (doc.RootElement.TryGetProperty("embedding", out var embedding) &&
                embedding.TryGetProperty("values", out var values))
            {
                var floats = new List<float>();
                foreach (var element in values.EnumerateArray())
                {
                    if (element.TryGetSingle(out var val))
                        floats.Add(val);
                }
                return floats;
            }

            throw new InvalidOperationException("Failed to parse embedding from API response");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to generate query embedding: {ex.Message}", ex);
        }
    }
}

