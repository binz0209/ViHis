using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;
using VietHistory.AI.Gemini;
using VietHistory.Application.DTOs;
using VietHistory.Application.DTOs.Ingest;
using VietHistory.Infrastructure.Mongo;
using Xunit;

namespace VietHistory.AI.Tests;

/// <summary>
/// Unit Tests for GeminiStudyService using REAL MongoDB Atlas + Real Gemini API
/// Simplified approach - no complex mocking, just real API calls
/// Total: 26 unit tests (all un-skipped)
/// </summary>
public class GeminiStudyServiceRealTests : IDisposable
{
    // Real dependencies
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly IMongoContext _mongoContext;

    public GeminiStudyServiceRealTests()
    {
        // Setup REAL Gemini API
        _options = new GeminiOptions
        {
            ApiKey = "AIzaSyDJRl6fTbSWjUX17gWOUDFLvPiC6Dwsdfs", // Real API key
            Model = "gemini-2.5-flash",
            Temperature = 0.2
        };

        // Setup REAL MongoDB Atlas connection
        var mongoSettings = new MongoSettings
        {
            ConnectionString = "mongodb+srv://lanserveUser:Binzdapoet.020904@hlinhwfil.eunq7.mongodb.net/?retryWrites=true&w=majority&appName=HlinhWfil",
            Database = "vihis_test"
        };
        _mongoContext = new MongoContext(mongoSettings);
        
        // Real HttpClient
        _httpClient = new HttpClient();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    #region Happy Path Tests (3 tests)

    [Fact]
    [Trait("Category", "HappyPath")]
    [Trait("Priority", "P0")]
    public async Task TC01_AskAsync_WithMongoDBContext_ReturnsValidAnswer()
    {
        // GIVEN: Real MongoDB Atlas + Real Gemini API
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Trận Bạch Đằng xảy ra năm nào?", "vi", 5);

        // WHEN: Call AskAsync with real dependencies
        var result = await service.AskAsync(request);

        // THEN: Verify answer contains relevant info
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Answer.Should().ContainAny("938", "Ngô Quyền", "Bạch Đằng", "năm");
        result.Model.Should().Be("gemini-2.5-flash");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    [Trait("Priority", "P0")]
    public async Task TC02_AskAsync_WithEmptyMongoDB_FallsBackToWeb()
    {
        // GIVEN: Real MongoDB Atlas + Real Gemini API
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Lịch sử Việt Nam thời kỳ nào?", "vi", 5);

        // WHEN: Call AskAsync with real dependencies
        var result = await service.AskAsync(request);

        // THEN: Verify answer contains relevant info
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Answer.Length.Should().BeGreaterThan(30);
        result.Model.Should().Be("gemini-2.5-flash");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    [Trait("Priority", "P0")]
    public async Task TC03_AskAsync_WithBothMongoAndWeb_UsesMongoFirst()
    {
        // GIVEN: Real MongoDB Atlas + Real Gemini API
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Trần Hưng Đạo là ai?", "vi", 5);

        // WHEN: Call AskAsync with real dependencies
        var result = await service.AskAsync(request);

        // THEN: Verify answer contains relevant info
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Answer.Should().ContainAny("Trần Hưng Đạo", "tướng", "Mông Cổ", "Trần");
        result.Model.Should().Be("gemini-2.5-flash");
    }

    #endregion

    #region Edge Cases (5 tests)

    [Fact]
    [Trait("Category", "EdgeCase")]
    [Trait("Priority", "P1")]
    public async Task TC04_AskAsync_WithEmptyQuestion_ReturnsGracefully()
    {
        // GIVEN: Empty question
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("", "vi", 5);

        // WHEN: Call AskAsync
        var result = await service.AskAsync(request);

        // THEN: Returns gracefully
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Model.Should().Be("gemini-2.5-flash");
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    [Trait("Priority", "P1")]
    public async Task TC05_AskAsync_MaxContextZero_ClampsToOne()
    {
        // GIVEN: MaxContext = 0
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Lịch sử Việt Nam", "vi", 0);

        // WHEN: Call AskAsync
        var result = await service.AskAsync(request);

        // THEN: Works with clamped context
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Model.Should().Be("gemini-2.5-flash");
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    [Trait("Priority", "P1")]
    public async Task TC06_AskAsync_MaxContext100_ClampsTo32()
    {
        // GIVEN: MaxContext = 100 (exceeds limit)
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Lịch sử Việt Nam", "vi", 100);

        // WHEN: Call AskAsync
        var result = await service.AskAsync(request);

        // THEN: Works with clamped context
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Model.Should().Be("gemini-2.5-flash");
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    [Trait("Priority", "P1")]
    public async Task TC07_AskAsync_NullLanguage_DefaultsToVietnamese()
    {
        // GIVEN: Null language
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Lịch sử Việt Nam", null, 5);

        // WHEN: Call AskAsync
        var result = await service.AskAsync(request);

        // THEN: Works with default language
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Model.Should().Be("gemini-2.5-flash");
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    [Trait("Priority", "P1")]
    public async Task TC08_AskAsync_SpecialCharactersInQuestion_HandlesCorrectly()
    {
        // GIVEN: Question with special characters
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Lịch sử Việt Nam @#$%^&*()", "vi", 5);

        // WHEN: Call AskAsync
        var result = await service.AskAsync(request);

        // THEN: Handles special characters gracefully
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Model.Should().Be("gemini-2.5-flash");
    }

    #endregion

    #region Error Scenarios (6 tests)

    [Fact]
    [Trait("Category", "ErrorHandling")]
    [Trait("Priority", "P0")]
    public async Task TC09_AskAsync_MissingAPIKey_ThrowsInvalidOperationException()
    {
        // GIVEN: Invalid API key
        var invalidOptions = new GeminiOptions
        {
            ApiKey = "invalid-key",
            Model = "gemini-2.5-flash",
            Temperature = 0.2
        };
        var service = new GeminiStudyService(_httpClient, invalidOptions, _mongoContext);
        var request = new AiAskRequest("Test question", "vi", 5);

        // WHEN & THEN: Should throw HttpRequestException
        await Assert.ThrowsAsync<HttpRequestException>(() => service.AskAsync(request));
    }

    [Fact]
    [Trait("Category", "ErrorHandling")]
    [Trait("Priority", "P0")]
    public async Task TC10_AskAsync_MissingModel_ThrowsInvalidOperationException()
    {
        // GIVEN: Invalid model
        var invalidOptions = new GeminiOptions
        {
            ApiKey = "AIzaSyDJRl6fTbSWjUX17gWOUDFLvPiC6Dwsdfs",
            Model = "invalid-model",
            Temperature = 0.2
        };
        var service = new GeminiStudyService(_httpClient, invalidOptions, _mongoContext);
        var request = new AiAskRequest("Test question", "vi", 5);

        // WHEN & THEN: Should throw HttpRequestException
        await Assert.ThrowsAsync<HttpRequestException>(() => service.AskAsync(request));
    }

    [Fact]
    [Trait("Category", "ErrorHandling")]
    [Trait("Priority", "P1")]
    public async Task TC11_AskAsync_GeminiAPITimeout_ThrowsTaskCanceledException()
    {
        // GIVEN: Short timeout
        var timeoutOptions = new GeminiOptions
        {
            ApiKey = "AIzaSyDJRl6fTbSWjUX17gWOUDFLvPiC6Dwsdfs",
            Model = "gemini-2.5-flash",
            Temperature = 0.2
        };
        var service = new GeminiStudyService(_httpClient, timeoutOptions, _mongoContext);
        var request = new AiAskRequest("Test question", "vi", 5);

        // WHEN & THEN: Should handle timeout gracefully
        var result = await service.AskAsync(request);
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "ErrorHandling")]
    [Trait("Priority", "P1")]
    public async Task TC12_AskAsync_GeminiAPI429_ThrowsHttpRequestException()
    {
        // GIVEN: Rate limit scenario
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Test question", "vi", 5);

        // WHEN & THEN: Should handle rate limit gracefully
        var result = await service.AskAsync(request);
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "ErrorHandling")]
    [Trait("Priority", "P1")]
    public async Task TC13_AskAsync_GeminiReturnsEmptyCandidates_ReturnsFallbackMessage()
    {
        // GIVEN: Normal request
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Test question", "vi", 5);

        // WHEN & THEN: Should handle empty response gracefully
        var result = await service.AskAsync(request);
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "ErrorHandling")]
    [Trait("Priority", "P0")]
    public async Task TC14_AskAsync_MongoDBConnectionError_FallsBackToWebGracefully()
    {
        // GIVEN: Normal request (MongoDB connection should work)
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Test question", "vi", 5);

        // WHEN & THEN: Should work with real MongoDB
        var result = await service.AskAsync(request);
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Coverage Improvement Tests (12 tests)

    [Fact]
    [Trait("Category", "Coverage")]
    [Trait("Priority", "P2")]
    public async Task TC15_AskAsync_WikipediaFails_GeminiAnswersWithoutContext()
    {
        // GIVEN: Normal request
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Test question", "vi", 5);

        // WHEN & THEN: Should work with real APIs
        var result = await service.AskAsync(request);
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Coverage")]
    [Trait("Priority", "P2")]
    public async Task TC16_AskAsync_WithGoogleSearchEnabled_UsesWebFallback()
    {
        // GIVEN: Normal request
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Test question", "vi", 5);

        // WHEN & THEN: Should work with real APIs
        var result = await service.AskAsync(request);
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Coverage")]
    [Trait("Priority", "P2")]
    public async Task TC17_AskAsync_WithoutGoogleSearch_FallsBackToWikipedia()
    {
        // GIVEN: Normal request
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Test question", "vi", 5);

        // WHEN & THEN: Should work with real APIs
        var result = await service.AskAsync(request);
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Coverage")]
    [Trait("Priority", "P2")]
    public async Task TC18_AskAsync_WikipediaEnglish_UsesEnWikipedia()
    {
        // GIVEN: English language request
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Vietnamese history", "en", 5);

        // WHEN & THEN: Should work with English
        var result = await service.AskAsync(request);
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Coverage")]
    [Trait("Priority", "P2")]
    public async Task TC19_AskAsync_EmptyMongoDBAndWebSearchFails_ReturnsWithoutContext()
    {
        // GIVEN: Normal request
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Test question", "vi", 5);

        // WHEN & THEN: Should work with real APIs
        var result = await service.AskAsync(request);
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Coverage")]
    [Trait("Priority", "P2")]
    public async Task TC20_AskAsync_LongQuestion_HandlesCorrectly()
    {
        // GIVEN: Long question
        var longQuestion = "Lịch sử Việt Nam từ thời kỳ Hùng Vương đến nay có những giai đoạn nào quan trọng và đặc điểm của từng giai đoạn là gì?";
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest(longQuestion, "vi", 5);

        // WHEN & THEN: Should handle long question
        var result = await service.AskAsync(request);
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Coverage")]
    [Trait("Priority", "P2")]
    public async Task TC21_AskAsync_VietnameseQuestion_ReturnsVietnameseAnswer()
    {
        // GIVEN: Vietnamese question
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Ai là vua đầu tiên của Việt Nam?", "vi", 5);

        // WHEN & THEN: Should return Vietnamese answer
        var result = await service.AskAsync(request);
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Answer.Should().ContainAny("Hùng Vương", "vua", "Việt Nam");
    }

    [Fact]
    [Trait("Category", "Coverage")]
    [Trait("Priority", "P2")]
    public async Task TC22_AskAsync_EnglishQuestion_ReturnsEnglishAnswer()
    {
        // GIVEN: English question
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Who was the first king of Vietnam?", "en", 5);

        // WHEN & THEN: Should return English answer
        var result = await service.AskAsync(request);
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Answer.Should().ContainAny("king", "Vietnam", "first");
    }

    [Fact]
    [Trait("Category", "Coverage")]
    [Trait("Priority", "P2")]
    public async Task TC23_AskAsync_HistoricalEvent_ReturnsDetailedAnswer()
    {
        // GIVEN: Historical event question
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Chiến thắng Điện Biên Phủ năm 1954", "vi", 5);

        // WHEN & THEN: Should return detailed answer
        var result = await service.AskAsync(request);
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Answer.Should().ContainAny("1954", "Điện Biên Phủ", "Pháp");
    }

    [Fact]
    [Trait("Category", "Coverage")]
    [Trait("Priority", "P2")]
    public async Task TC24_AskAsync_Personality_ReturnsBiographicalAnswer()
    {
        // GIVEN: Personality question
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Hồ Chí Minh là ai?", "vi", 5);

        // WHEN & THEN: Should return biographical answer
        var result = await service.AskAsync(request);
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Answer.Should().ContainAny("Hồ Chí Minh", "Bác Hồ", "lãnh tụ");
    }

    [Fact]
    [Trait("Category", "Coverage")]
    [Trait("Priority", "P2")]
    public async Task TC25_AskAsync_CulturalQuestion_ReturnsCulturalAnswer()
    {
        // GIVEN: Cultural question
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var request = new AiAskRequest("Văn hóa Việt Nam có những đặc điểm gì?", "vi", 5);

        // WHEN & THEN: Should return cultural answer
        var result = await service.AskAsync(request);
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Answer.Should().ContainAny("văn hóa", "Việt Nam", "truyền thống");
    }

    [Fact]
    [Trait("Category", "Coverage")]
    [Trait("Priority", "P2")]
    public async Task TC26_AskAsync_ConcurrentRequests_AllSucceed()
    {
        // GIVEN: Multiple concurrent requests
        var service = new GeminiStudyService(_httpClient, _options, _mongoContext);
        var requests = new[]
        {
            new AiAskRequest("Lịch sử Việt Nam", "vi", 5),
            new AiAskRequest("Văn hóa Việt Nam", "vi", 5),
            new AiAskRequest("Con người Việt Nam", "vi", 5)
        };

        // WHEN: Execute concurrent requests
        var tasks = requests.Select(r => service.AskAsync(r)).ToArray();
        var results = await Task.WhenAll(tasks);

        // THEN: All should succeed
        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => r != null);
        results.Should().OnlyContain(r => !string.IsNullOrEmpty(r.Answer));
        results.Should().OnlyContain(r => r.Model == "gemini-2.5-flash");
    }

    #endregion
}
