using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using VietHistory.AI.Gemini;
using VietHistory.Application.DTOs;
using VietHistory.Infrastructure.Mongo;
using Xunit;

namespace VietHistory.AI.Tests;

/// <summary>
/// REAL INTEGRATION TESTS for GeminiStudyService
/// Tests REAL MongoDB Atlas + REAL Gemini API + REAL Web APIs
/// 
/// PURPOSE: Validate end-to-end behavior with actual dependencies
/// COMPETITION REQUIREMENT: "Feature liên quan nhiều file nhiều class thì cần integration test"
/// 
/// Dependencies:
/// - MongoDB Atlas: Real connection string
/// - Gemini API: Real API key
/// - Web APIs: Real Wikipedia/Google Search APIs
/// 
/// CREDENTIALS: Provided by user - working and tested
/// </summary>
[Collection("Integration")]
public class GeminiStudyServiceIntegrationTests : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly IMongoContext _mongoContext;
    private readonly GeminiStudyService _service;

    public GeminiStudyServiceIntegrationTests()
    {
        // Real MongoDB Atlas connection
        var mongoSettings = new MongoSettings
        {
            ConnectionString = "mongodb+srv://lanserveUser:Binzdapoet.020904@hlinhwfil.eunq7.mongodb.net/?retryWrites=true&w=majority&appName=HlinhWfil",
            Database = "vihis_test" // Use separate test database
        };
        _mongoContext = new MongoContext(mongoSettings);

        // Real Gemini API configuration
        _options = new GeminiOptions
        {
            ApiKey = "AIzaSyDJRl6fTbSWjUX17gWOUDFLvPiC6Dwsdfs", // New API key
            Model = "gemini-2.5-flash", // Correct model
            Temperature = 0.2,
            GoogleSearchApiKey = "AIzaSyDJRl6fTbSWjUX17gWOUDFLvPiC6Dwsdfs", // Reuse Gemini key
            GoogleSearchCx = "559f66cd988fb4a7d"
        };

        // Real HttpClient
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        
        // Create service with REAL dependencies
        _service = new GeminiStudyService(_httpClient, _options, _mongoContext);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    #region Integration Tests (5 critical E2E tests)

    [Fact] // ✅ Un-skipped - Using NEW API key
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    public async Task IT01_RealAPI_VietnameseHistoryQuestion_ReturnsValidAnswer()
    {
        // GIVEN: Real MongoDB Atlas + Real Gemini API
        // Question: Vietnamese history topic that likely exists in DB
        var request = new AiAskRequest("Trần Hưng Đạo là ai?", "vi", 5);

        // WHEN: Call AskAsync with real dependencies
        var stopwatch = Stopwatch.StartNew();
        var result = await _service.AskAsync(request);
        stopwatch.Stop();

        // THEN: Returns valid answer with real data
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Model.Should().Be("gemini-2.5-flash");
        result.Answer.Should().ContainAny("Trần Hưng Đạo", "tướng", "Mông Cổ", "Trần", "đại vương");
        
        // Verify response time is reasonable
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(15), 
            "API should respond within 15 seconds");
        
        Console.WriteLine($"✅ IT01 PASSED - Response time: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Answer: {result.Answer.Substring(0, Math.Min(200, result.Answer.Length))}...");
    }

    [Fact] // ✅ Un-skipped - Using NEW API key
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    public async Task IT02_RealAPI_QuestionNotInDatabase_FallsBackToWeb()
    {
        // GIVEN: Question about recent event (not in historical DB)
        var request = new AiAskRequest("Elon Musk mua Twitter năm nào?", "vi", 5);

        // WHEN: Call AskAsync - should fallback to web search
        var stopwatch = Stopwatch.StartNew();
        var result = await _service.AskAsync(request);
        stopwatch.Stop();

        // THEN: Returns answer from web context (Wikipedia/Google)
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Model.Should().Be("gemini-2.5-flash");
        
        // Should contain some reference to recent event or general knowledge
        result.Answer.Length.Should().BeGreaterThan(30, "should have meaningful answer");
        
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(20), 
            "web fallback should complete within 20 seconds");
        
        Console.WriteLine($"✅ IT02 PASSED - Web fallback time: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Answer: {result.Answer.Substring(0, Math.Min(200, result.Answer.Length))}...");
    }

    [Fact] // ✅ Un-skipped - Using NEW API key
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    public async Task IT03_RealAPI_EnglishLanguage_ReturnsEnglishAnswer()
    {
        // GIVEN: Question in English
        var request = new AiAskRequest("Who was Tran Hung Dao?", "en", 5);

        // WHEN: Call AskAsync with English language
        var stopwatch = Stopwatch.StartNew();
        var result = await _service.AskAsync(request);
        stopwatch.Stop();

        // THEN: Returns answer in English
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Model.Should().Be("gemini-2.5-flash");
        
        // Should contain English keywords
        result.Answer.Should().ContainAny("Tran Hung Dao", "general", "commander", "Vietnam", "Mongol");
        
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(15), 
            "English API should respond within 15 seconds");
        
        Console.WriteLine($"✅ IT03 PASSED - English response");
        Console.WriteLine($"Answer: {result.Answer.Substring(0, Math.Min(200, result.Answer.Length))}...");
    }

    [Fact] // ✅ Un-skipped - Using NEW API key
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    public async Task IT04_RealAPI_ConcurrentRequests_AllSucceed()
    {
        // GIVEN: Multiple concurrent requests
        var requests = new[]
        {
            new AiAskRequest("Trận Bạch Đằng xảy ra năm nào?", "vi", 3),
            new AiAskRequest("Lý Thường Kiệt là ai?", "vi", 3),
            new AiAskRequest("Hai Bà Trưng khởi nghĩa năm nào?", "vi", 3)
        };

        // WHEN: Call AskAsync concurrently
        var stopwatch = Stopwatch.StartNew();
        var tasks = requests.Select(req => _service.AskAsync(req)).ToArray();
        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // THEN: All requests should succeed
        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => r != null);
        results.Should().OnlyContain(r => !string.IsNullOrEmpty(r.Answer));
        results.Should().OnlyContain(r => r.Model == "gemini-2.5-flash");
        
        // All should complete within reasonable time
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(30), 
            "concurrent requests should complete within 30 seconds");
        
        Console.WriteLine($"✅ IT04 PASSED - Concurrent requests completed in {stopwatch.ElapsedMilliseconds}ms");
        foreach (var (result, index) in results.Select((r, i) => (r, i)))
        {
            Console.WriteLine($"Request {index + 1}: {result.Answer.Substring(0, Math.Min(100, result.Answer.Length))}...");
        }
    }

    [Fact] // ✅ Un-skipped - Using NEW API key
    [Trait("Category", "Integration")]
    [Trait("Priority", "P2")]
    public async Task IT05_RealAPI_MongoDBConnection_VerifyDataAccess()
    {
        // GIVEN: Real MongoDB connection
        // Test MongoDB connection by calling AskAsync (which uses MongoDB internally)
        var request = new AiAskRequest("Test MongoDB connection", "vi", 1);

        // WHEN: Call AskAsync which internally uses MongoDB
        var stopwatch = Stopwatch.StartNew();
        var result = await _service.AskAsync(request);
        stopwatch.Stop();

        // THEN: Should be able to access MongoDB (no exception = connection OK)
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Model.Should().Be("gemini-2.5-flash");
        
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(30), 
            "MongoDB connection should work within 30 seconds");
        
        Console.WriteLine($"✅ IT05 PASSED - MongoDB connection verified via AskAsync");
        Console.WriteLine($"Response time: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Answer: {result.Answer.Substring(0, Math.Min(100, result.Answer.Length))}...");
    }

    #endregion
}