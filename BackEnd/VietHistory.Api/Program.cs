using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using VietHistory.Infrastructure.Services.Gemini;
using VietHistory.Application.Services;
using VietHistory.Infrastructure.Mongo;
using VietHistory.Infrastructure.Services;
using VietHistory.Infrastructure.Services.AI;
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
builder.Services.AddSingleton<IMongoContext>(sp => sp.GetRequiredService<MongoContext>());
builder.Services.AddSingleton<ISourceRepository, SourceRepository>();
builder.Services.AddSingleton<IChunkRepository, ChunkRepository>();

// ================= Gemini (AI) =================
var geminiOptions = new GeminiOptions
{
    ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
             ?? builder.Configuration["Gemini:ApiKey"] ?? "",
    Model = builder.Configuration["Gemini:Model"] ?? "gemini-2.5-flash",
    Temperature = double.TryParse(builder.Configuration["Gemini:Temperature"], out var t) ? t : 0.2,

    GoogleSearchApiKey = Environment.GetEnvironmentVariable("GOOGLE_CSE_KEY")
             ?? builder.Configuration["Gemini:GoogleSearchApiKey"],
    GoogleSearchCx = Environment.GetEnvironmentVariable("GOOGLE_CSE_CX")
             ?? builder.Configuration["Gemini:GoogleSearchCx"]
};
builder.Services.AddSingleton(geminiOptions);

// Register Embedding Service (optional, fallback to text search if not available)
var embeddingApiKey = builder.Configuration["Gemini:ApiKey"] ?? "";
var embeddingModel = builder.Configuration["Gemini:EmbeddingModel"] ?? "text-embedding-004";
builder.Services.AddHttpClient<EmbeddingService>(c => c.Timeout = TimeSpan.FromSeconds(60));
builder.Services.AddScoped(sp =>
{
    if (!string.IsNullOrEmpty(embeddingApiKey))
    {
        var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
        var http = httpFactory.CreateClient(nameof(EmbeddingService));
        return new EmbeddingService(http, embeddingApiKey, embeddingModel);
    }
    return null as EmbeddingService;
});

builder.Services.AddHttpClient<IAIStudyService, GeminiStudyService>(c => c.Timeout = TimeSpan.FromSeconds(60));
builder.Services.AddScoped<IAIStudyService, GeminiStudyService>();

// Register KWideRetriever with EmbeddingService (if available)
builder.Services.AddScoped(sp =>
{
    var ctx = sp.GetRequiredService<IMongoContext>();
    var embeddingService = sp.GetRequiredService<EmbeddingService?>();
    return embeddingService != null 
        ? new KWideRetriever(ctx, embeddingService)
        : new KWideRetriever(ctx);
});

// ================= Ingest Services (PDF → chunks) =================
builder.Services.AddSingleton<IPdfTextExtractor, PdfTextExtractor>();
builder.Services.AddSingleton<IFallbackAIngestor, FallbackAIngestor>();

// ================= App services (domain) =================
builder.Services.AddScoped<IPeopleService, PeopleService>();
builder.Services.AddScoped<IEventsService, EventsService>();

// ================= JWT Authentication =================
var jwtOptions = new JwtOptions
{
    Key = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing"),
    Issuer = builder.Configuration["Jwt:Issuer"] ?? "VietHistory.Api",
    Audience = builder.Configuration["Jwt:Audience"] ?? "VietHistory.Client",
    ExpirationMinutes = int.TryParse(builder.Configuration["Jwt:ExpirationMinutes"], out var exp) ? exp : 60
};
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<JwtService>();

// Configure JWT Authentication
var key = Encoding.UTF8.GetBytes(jwtOptions.Key);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ================= CORS =================
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000", "http://localhost:5173", "http://localhost:3001" };

builder.Services.AddCors(options =>
{
    // Development: Allow các origin cụ thể với credentials
    options.AddPolicy("Development",
        policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("*"));

    // Production: Allow origins trong config + Vercel domains
    options.AddPolicy("Production",
        policy => policy
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .WithOrigins(allowedOrigins)
            .SetIsOriginAllowed(origin =>
            {
                // Allow localhost
                if (origin.Contains("localhost") || origin.StartsWith("http://127.0.0.1"))
                    return true;
                
                // Allow Vercel domains
                if (origin.Contains("vercel.app"))
                    return true;
                
                // Allow specific origins
                return allowedOrigins.Contains(origin);
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("*"));

    // Fallback: Allow tất cả (nếu cần cho integration testing)
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// ================= Controllers + Swagger =================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "VietHistory API", Version = "v1" });
});

var app = builder.Build();

// ================= Middleware =================
app.UseSwagger();
app.UseSwaggerUI();

// CORS: Dùng Development cho dev, Production cho production
var environment = app.Environment;
if (environment.IsDevelopment())
{
    app.UseCors("Development");
    Console.WriteLine($"✅ CORS Development enabled for: {string.Join(", ", allowedOrigins)}");
}
else
{
    app.UseCors("Production");
    Console.WriteLine($"✅ CORS Production enabled for: {string.Join(", ", allowedOrigins)}");
}

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ================= Warm-up Mongo =================
Task.Run(async () =>
{
    try
    {
        // Lấy context từ DI container
        using var scope = app.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<IMongoContext>();

        // Gọi ping để khởi tạo kết nối
        await ctx.PingAsync();

        // Truy vấn 1 document nhỏ để warm-up metadata
        await ctx.Chunks.Find(_ => true).Limit(1).FirstOrDefaultAsync();

        Console.WriteLine("✅ MongoDB warm-up completed.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Mongo warm-up failed: {ex.Message}");
    }
});

app.Run();
