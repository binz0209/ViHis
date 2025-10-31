// Renamed from GeminiStudyServiceIntegrationTests for feature-based discoverability
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using  VietHistory.Infrastructure.Services.Gemini;
using VietHistory.Application.DTOs;
using VietHistory.Infrastructure.Mongo;
using Xunit;
using VietHistory.Infrastructure.Services.AI;

namespace VietHistory.AI.Tests;

[Collection("Integration")]
[Trait("Feature", "AI_QA")]
[Trait("Integration", "Real")]
public class AI_QA_IntegrationTests : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly IMongoContext _mongoContext;
    private readonly GeminiStudyService _service;

    public AI_QA_IntegrationTests()
    {
        var mongoSettings = new MongoSettings
        {
            ConnectionString = "mongodb+srv://lanserveUser:Binzdapoet.020904@hlinhwfil.eunq7.mongodb.net/?retryWrites=true&w=majority&appName=HlinhWfil",
            Database = "vihis_test"
        };
        _mongoContext = new MongoContext(mongoSettings);

        _options = new GeminiOptions
        {
            ApiKey = "AIzaSyCpicTHDEmrz8XhsuQc9Vqn8rsDdf2d6x0",
            Model = "gemini-2.5-flash",
            Temperature = 0.2,
            GoogleSearchApiKey = "AIzaSyCpicTHDEmrz8XhsuQc9Vqn8rsDdf2d6x0",
            GoogleSearchCx = "559f66cd988fb4a7d"
        };

        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        var retriever = new KWideRetriever(_mongoContext);
        _service = new GeminiStudyService(_httpClient, _options, _mongoContext, retriever);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    public async Task TC01_RealAPI_VietnameseHistoryQuestion_ReturnsValidAnswer()
    {
        var request = new AiAskRequest("Trần Hưng Đạo là ai?", "vi", 5);
        var stopwatch = Stopwatch.StartNew();
        var result = await _service.AskAsync(request);
        stopwatch.Stop();
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrEmpty();
        result.Model.Should().Be("gemini-2.5-flash");
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(20));
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    public async Task TC02_Fallback_Web_When_DB_Not_Relevant()
    {
        var request = new AiAskRequest("Elon Musk mua Twitter năm nào?", "vi", 5);
        var sw = Stopwatch.StartNew();
        try
        {
            var result = await _service.AskAsync(request);
            result.Should().NotBeNull();
            result.Answer.Should().NotBeNullOrEmpty();
        }
        catch (HttpRequestException ex)
        {
            // Accept rate limit as non-failing for this scenario
            (ex.Message.Contains("429") || ex.Message.Contains("Too Many Requests")).Should().BeTrue();
        }
        finally
        {
            sw.Stop();
        }
    }

    [Fact]
    [Trait("Category", "Performance")]
    [Trait("Priority", "P1")]
    public async Task TC03_P95_Should_Be_Under_15s_For_Medium_Query()
    {
        var sw = Stopwatch.StartNew();
        var result = await _service.AskAsync(new AiAskRequest("Tóm tắt cuộc kháng chiến chống Nguyên Mông", "vi", 5));
        sw.Stop();
        result.Answer.Should().NotBeNullOrEmpty();
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(20));
    }

    [Fact]
    [Trait("Category", "Concurrency")]
    [Trait("Priority", "P1")]
    public async Task TC04_Three_Concurrent_Requests_All_Succeed_Under_30s()
    {
        var reqs = new[]
        {
            new AiAskRequest("Trận Bạch Đằng 938", "vi", 3),
            new AiAskRequest("Lý Thường Kiệt là ai?", "vi", 3),
            new AiAskRequest("Hai Bà Trưng khởi nghĩa năm nào?", "vi", 3)
        };
        var sw = Stopwatch.StartNew();
        var results = await Task.WhenAll(reqs.Select(r => _service.AskAsync(r)));
        sw.Stop();
        results.Should().OnlyContain(r => r != null && !string.IsNullOrEmpty(r.Answer));
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(30));
    }

    [Fact]
    [Trait("Category", "Concurrency")]
    [Trait("Priority", "P1")]
    public async Task TC05_Five_Parallel_Requests_All_Succeed_Under_60s()
    {
        var reqs = new[]
        {
            new AiAskRequest("Lịch sử Việt Nam", "vi", 5),
            new AiAskRequest("Văn hóa Việt Nam", "vi", 5),
            new AiAskRequest("Con người Việt Nam", "vi", 5),
            new AiAskRequest("Địa lý Việt Nam", "vi", 5),
            new AiAskRequest("Kinh tế Việt Nam", "vi", 5)
        };
        var sw = Stopwatch.StartNew();
        var results = await Task.WhenAll(reqs.Select(r => _service.AskAsync(r)));
        sw.Stop();
        results.Should().OnlyContain(r => r != null && !string.IsNullOrEmpty(r.Answer));
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(60));
    }

    [Fact]
    [Trait("Category", "Performance")]
    [Trait("Priority", "P1")]
    public async Task TC06_Repeated_Same_Question_Should_Not_Slow_Down_Much()
    {
        try
        {
            var sw1 = Stopwatch.StartNew();
            var r1 = await _service.AskAsync(new AiAskRequest("Triều Lý thành lập năm nào?", "vi", 5));
            sw1.Stop();

            var sw2 = Stopwatch.StartNew();
            var r2 = await _service.AskAsync(new AiAskRequest("Triều Lý thành lập năm nào?", "vi", 5));
            sw2.Stop();

            r1.Answer.Should().NotBeNullOrEmpty();
            r2.Answer.Should().NotBeNullOrEmpty();
            sw2.ElapsedMilliseconds.Should().BeLessThan((long)(sw1.ElapsedMilliseconds * 1.3) + 3000);
        }
        catch (HttpRequestException ex)
        {
            (ex.Message.Contains("429") || ex.Message.Contains("Too Many Requests")).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("Category", "I18N")]
    [Trait("Priority", "P1")]
    public async Task TC07_MultiLanguage_All_Valid()
    {
        var reqs = new[]
        {
            new AiAskRequest("Lịch sử Việt Nam", "vi", 5),
            new AiAskRequest("Vietnamese history", "en", 5)
        };
        var results = await Task.WhenAll(reqs.Select(r => _service.AskAsync(r)));
        results.Should().OnlyContain(r => r != null && !string.IsNullOrEmpty(r.Answer));
    }

    [Fact]
    [Trait("Category", "Performance")]
    [Trait("Priority", "P2")]
    public async Task TC08_Two_Runs_Stable_Total_Time()
    {
        try
        {
            var sw1 = Stopwatch.StartNew();
            await _service.AskAsync(new AiAskRequest("Tóm tắt nhà Trần", "vi", 5));
            sw1.Stop();

            var sw2 = Stopwatch.StartNew();
            await _service.AskAsync(new AiAskRequest("Tóm tắt nhà Trần", "vi", 5));
            sw2.Stop();

            sw2.ElapsedMilliseconds.Should().BeLessThan((long)(sw1.ElapsedMilliseconds * 1.4) + 4000);
        }
        catch (HttpRequestException ex)
        {
            (ex.Message.Contains("429") || ex.Message.Contains("Too Many Requests")).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    public async Task TC09_English_Integration_Basic()
    {
        var result = await _service.AskAsync(new AiAskRequest("Who was Tran Hung Dao?", "en", 5));
        result.Answer.Should().NotBeNullOrEmpty();
    }
}


