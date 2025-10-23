using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using VietHistory.Application.DTOs;
using VietHistory.Application.Services;
using VietHistory.Infrastructure.Mongo;
using VietHistory.Application.DTOs.Ingest;

namespace VietHistory.AI.Gemini;

public class GeminiOptions
{
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "gemini-2.5-flash";
    public double Temperature { get; set; } = 0.2;

    // (Tuỳ chọn) Google Programmable Search
    public string? GoogleSearchApiKey { get; set; } = null; // e.g. "AIza..."
    public string? GoogleSearchCx { get; set; } = null;     // e.g. "abcd1234:xxxx"
}

public class GeminiStudyService : IAIStudyService
{
    private static readonly JsonSerializerOptions JsonOpt = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private readonly GeminiOptions _opt;
    private readonly MongoContext _ctx;
    private static bool _indexesEnsured = false;

    public GeminiStudyService(HttpClient http, GeminiOptions opt, MongoContext ctx)
    {
        _http = http;
        _opt = opt;
        _ctx = ctx;
        _http.Timeout = TimeSpan.FromSeconds(60);
    }

    public async Task<AiAnswer> AskAsync(AiAskRequest req) => await AskAsync(req, CancellationToken.None);

    public async Task<AiAnswer> AskAsync(AiAskRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_opt.ApiKey))
            throw new InvalidOperationException("Missing Gemini API key. Set GEMINI_API_KEY or appsettings:Gemini:ApiKey.");

        if (string.IsNullOrWhiteSpace(_opt.Model))
            throw new InvalidOperationException("Missing Gemini model. Set appsettings:Gemini:Model.");

        await EnsureChunkTextIndexOnce(ct);

        // 1) RAG từ Mongo
        var max = Math.Clamp(req.MaxContext <= 0 ? 12 : req.MaxContext, 1, 32);
        var chunks = await QueryTopChunksAsync(req.Question ?? "", null, max, ct);
        var mongoContext = await BuildChunkContextAsync(chunks, ct);

        // 2) Nếu không có context trong tài liệu → tìm trên web
        List<WebSnippet> webSnippets = new();
        if (string.IsNullOrWhiteSpace(mongoContext))
        {
            webSnippets = await SearchWebAsync(req.Question ?? "", req.Language, 3, ct);
        }

        // 3) Prompt tổng hợp
        var lang = string.IsNullOrWhiteSpace(req.Language) ? "vi" : req.Language!;
        var systemPrompt =
            $"Bạn là trợ lý học tập lịch sử Việt Nam. Luôn trả lời bằng ngôn ngữ: {lang}. " +
            "Hãy sử dụng BỐI CẢNH từ tài liệu đã học (nếu có). " +
            "Nếu không có, hãy dựa vào BỐI CẢNH WEB được trích dưới đây và kiến thức chung để trả lời ngắn gọn, chính xác. " +
            "Khi phù hợp, nhắc tên nguồn (tiêu đề + trang hoặc URL) trong ngoặc vuông.";

        var parts = new List<object> { new { text = systemPrompt } };

        if (!string.IsNullOrWhiteSpace(mongoContext))
            parts.Add(new { text = "Bối cảnh (tài liệu PDF đã ingest):\n" + mongoContext });
        if (webSnippets.Count > 0)
            parts.Add(new { text = "Bối cảnh Web (tóm tắt):\n" + JoinWebSnippets(webSnippets) });

        parts.Add(new { text = "Câu hỏi: " + (req.Question ?? "") });

        var body = new
        {
            contents = new[] { new { role = "user", parts = parts.ToArray() } },
            generationConfig = new { temperature = _opt.Temperature }
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_opt.Model}:generateContent?key={_opt.ApiKey}";
        using var httpContent = new StringContent(JsonSerializer.Serialize(body, JsonOpt), Encoding.UTF8, "application/json");
        using var resp = await _http.PostAsync(url, httpContent, ct);
        var respText = await resp.Content.ReadAsStringAsync(ct);
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(respText);
        var root = doc.RootElement;
        var answer = ExtractText(root) ?? "(Không nhận được câu trả lời từ mô hình.)";

        return new AiAnswer(answer, _opt.Model, null);
    }

    // ======================== RAG: Mongo ========================

    private async Task EnsureChunkTextIndexOnce(CancellationToken ct)
    {
        if (_indexesEnsured) return;
        try
        {
            var keys = Builders<ChunkDoc>.IndexKeys
                .Ascending(x => x.SourceId)
                .Ascending(x => x.ChunkIndex);
            await _ctx.Chunks.Indexes.CreateOneAsync(new CreateIndexModel<ChunkDoc>(keys), cancellationToken: ct);

            var textIdx = Builders<ChunkDoc>.IndexKeys.Text(x => x.Content);
            await _ctx.Chunks.Indexes.CreateOneAsync(new CreateIndexModel<ChunkDoc>(textIdx), cancellationToken: ct);
        }
        catch
        {
            // Không sao nếu không có quyền tạo index
        }
        finally { _indexesEnsured = true; }
    }

    private async Task<List<ChunkDoc>> QueryTopChunksAsync(string question, string? sourceId, int limit, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(question))
            return new List<ChunkDoc>();

        // $text search
        try
        {
            var filter = Builders<ChunkDoc>.Filter.Text(question);
            if (!string.IsNullOrWhiteSpace(sourceId))
                filter &= Builders<ChunkDoc>.Filter.Eq(x => x.SourceId, sourceId);

            var sort = Builders<ChunkDoc>.Sort.MetaTextScore("score");
            var cursor = await _ctx.Chunks.FindAsync(filter, new FindOptions<ChunkDoc> { Sort = sort, Limit = limit }, ct);
            var list = await cursor.ToListAsync(ct);
            if (list.Count > 0) return list;
        }
        catch { /* fallback */ }

        // Regex fallback
        var regex = new BsonRegularExpression(System.Text.RegularExpressions.Regex.Escape(question), "i");
        var fbFilter = Builders<ChunkDoc>.Filter.Regex(x => x.Content, regex);
        if (!string.IsNullOrWhiteSpace(sourceId))
            fbFilter &= Builders<ChunkDoc>.Filter.Eq(x => x.SourceId, sourceId);
        return await _ctx.Chunks.Find(fbFilter).Limit(limit).ToListAsync(ct);
    }

    private async Task<string> BuildChunkContextAsync(List<ChunkDoc> chunks, CancellationToken ct)
    {
        if (chunks.Count == 0) return string.Empty;
        var srcIds = chunks.Select(c => c.SourceId).Distinct().ToList();
        var srcDocs = await _ctx.Sources.Find(s => srcIds.Contains(s.Id)).ToListAsync(ct);
        var srcMap = srcDocs.ToDictionary(s => s.Id, s => s.Title ?? s.FileName);

        var lines = new List<string>(chunks.Count);
        foreach (var c in chunks)
        {
            var title = srcMap.TryGetValue(c.SourceId, out var t) ? t : "Nguồn chưa rõ";
            var pages = c.PageFrom == c.PageTo ? $"{c.PageFrom}" : $"{c.PageFrom}-{c.PageTo}";
            var snip = Truncate(OneLine(c.Content), 900);
            lines.Add($"• [{title} – Trang {pages}] {snip}");
        }
        return string.Join("\n", lines);
    }

    // ======================== WEB FALLBACK ========================

    private record WebSnippet(string Title, string Url, string Snippet);

    private async Task<List<WebSnippet>> SearchWebAsync(string query, string? language, int max, CancellationToken ct)
    {
        var results = new List<WebSnippet>();

        // 1) Nếu có Google Programmable Search → dùng trước
        if (!string.IsNullOrWhiteSpace(_opt.GoogleSearchApiKey) && !string.IsNullOrWhiteSpace(_opt.GoogleSearchCx))
        {
            try
            {
                var url = $"https://www.googleapis.com/customsearch/v1?q={Uri.EscapeDataString(query)}&num={Math.Clamp(max, 1, 10)}&key={_opt.GoogleSearchApiKey}&cx={_opt.GoogleSearchCx}";
                using var resp = await _http.GetAsync(url, ct);
                if (resp.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
                    if (doc.RootElement.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var it in items.EnumerateArray())
                        {
                            var title = it.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
                            var link = it.TryGetProperty("link", out var l) ? l.GetString() ?? "" : "";
                            var snip = it.TryGetProperty("snippet", out var s) ? s.GetString() ?? "" : "";
                            if (!string.IsNullOrWhiteSpace(link))
                                results.Add(new WebSnippet(title, link, snip));
                        }
                    }
                }
            }
            catch { /* bỏ qua và fallback */ }
        }

        // 2) Wikipedia (vi/en) fallback (không cần API key)
        if (results.Count == 0)
        {
            var lang = string.IsNullOrWhiteSpace(language) ? "vi" : language!.StartsWith("vi", StringComparison.OrdinalIgnoreCase) ? "vi" : "en";
            try
            {
                // search
                var searchUrl = $"https://{lang}.wikipedia.org/w/api.php?action=opensearch&search={Uri.EscapeDataString(query)}&limit={Math.Clamp(max, 1, 10)}&namespace=0&format=json";
                using var sresp = await _http.GetAsync(searchUrl, ct);
                if (sresp.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(await sresp.Content.ReadAsStringAsync(ct));
                    // format: [search term, titles[], descriptions[], urls[]]
                    if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() >= 4)
                    {
                        var titles = doc.RootElement[1];
                        var descs = doc.RootElement[2];
                        var links = doc.RootElement[3];
                        for (int i = 0; i < Math.Min(Math.Min(titles.GetArrayLength(), descs.GetArrayLength()), links.GetArrayLength()); i++)
                        {
                            var title = titles[i].GetString() ?? "";
                            var link = links[i].GetString() ?? "";
                            var snip = descs[i].GetString() ?? "";

                            // summary (REST)
                            string summary = snip;
                            if (!string.IsNullOrWhiteSpace(title))
                            {
                                var sumUrl = $"https://{lang}.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(title)}";
                                try
                                {
                                    using var sumResp = await _http.GetAsync(sumUrl, ct);
                                    if (sumResp.IsSuccessStatusCode)
                                    {
                                        using var sumDoc = JsonDocument.Parse(await sumResp.Content.ReadAsStringAsync(ct));
                                        if (sumDoc.RootElement.TryGetProperty("extract", out var ex))
                                            summary = ex.GetString() ?? summary;
                                    }
                                }
                                catch { /* ignore */ }
                            }

                            if (!string.IsNullOrWhiteSpace(link))
                                results.Add(new WebSnippet(title, link, summary));
                        }
                    }
                }
            }
            catch { /* ignore */ }
        }

        return results.Take(max).ToList();
    }

    private static string JoinWebSnippets(List<WebSnippet> items)
    {
        var lines = items.Select(i => $"• [{i.Title}] {i.Snippet} (Nguồn: {i.Url})");
        return string.Join("\n", lines);
    }

    // ======================== Helpers ========================

    private static string OneLine(string s) => (s ?? string.Empty).Replace("\r\n", " ").Replace('\n', ' ').Trim();
    private static string Truncate(string s, int max) => s.Length <= max ? s : s.Substring(0, max) + "…";

    private static string? ExtractText(JsonElement root)
    {
        if (!root.TryGetProperty("candidates", out var cand) || cand.ValueKind != JsonValueKind.Array || cand.GetArrayLength() == 0)
            return null;
        var first = cand[0];
        if (!first.TryGetProperty("content", out var content)) return null;

        var sb = new StringBuilder();
        if (content.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
        {
            foreach (var p in parts.EnumerateArray())
                if (p.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String)
                    sb.Append(t.GetString());
        }
        if (sb.Length == 0 && first.TryGetProperty("text", out var t2) && t2.ValueKind == JsonValueKind.String)
            sb.Append(t2.GetString());
        var text = sb.ToString();
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }
}
