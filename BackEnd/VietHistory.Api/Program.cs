using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;
using VietHistory.AI.Gemini;
using VietHistory.Application.Services;
using VietHistory.Infrastructure.Mongo;
using VietHistory.Infrastructure.Services;
using VietHistory.Infrastructure.Services.TextIngest;

var builder = WebApplication.CreateBuilder(args);

// ================= Upload limit (300 MB) =================
const long UPLOAD_LIMIT = 300L * 1024 * 1024; // 300 MB

builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxRequestBodySize = UPLOAD_LIMIT;
});

builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = UPLOAD_LIMIT;
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartHeadersLengthLimit = int.MaxValue;
});

// ================= Mongo =================
var mongoSettings = new MongoSettings();
builder.Configuration.GetSection("Mongo").Bind(mongoSettings);
builder.Services.AddSingleton(mongoSettings);
builder.Services.AddSingleton<MongoContext>();

// Repos chuyên biệt cho ingest (sources/chunks)
builder.Services.AddSingleton<ISourceRepository, SourceRepository>();
builder.Services.AddSingleton<IChunkRepository, ChunkRepository>();

// ================= Gemini (AI) =================
var geminiOptions = new GeminiOptions
{
    ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
             ?? builder.Configuration["Gemini:ApiKey"] ?? "",
    Model = builder.Configuration["Gemini:Model"] ?? "gemini-2.5-flash",
    Temperature = double.TryParse(builder.Configuration["Gemini:Temperature"], out var t) ? t : 0.2,

    // Tùy chọn: Google Programmable Search (nếu muốn web search xịn hơn)
    GoogleSearchApiKey = Environment.GetEnvironmentVariable("GOOGLE_CSE_KEY")
             ?? builder.Configuration["Gemini:GoogleSearchApiKey"],
    GoogleSearchCx = Environment.GetEnvironmentVariable("GOOGLE_CSE_CX")
             ?? builder.Configuration["Gemini:GoogleSearchCx"]
};
builder.Services.AddSingleton(geminiOptions);

// HttpClient dùng cho cả Gemini lẫn web search
builder.Services.AddHttpClient<GeminiStudyService>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(60);
});
builder.Services.AddScoped<IAIStudyService, GeminiStudyService>();

// ================= Ingest Services (PDF → chunks) =================
builder.Services.AddSingleton<IPdfTextExtractor, PdfTextExtractor>(); // PdfPig extractor
builder.Services.AddSingleton<IFallbackAIngestor, FallbackAIngestor>();

// ================= App services (domain) =================
builder.Services.AddScoped<IPeopleService, PeopleService>();
builder.Services.AddScoped<IEventsService, EventsService>();

// ================= Controllers + Swagger =================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "VietHistory API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
