# 📋 GIAI ĐOẠN 1: PHÂN TÍCH FEATURE - GeminiStudyService

**Thời gian**: 15 phút  
**Core Feature**: AI Historical Q&A với RAG (Retrieval-Augmented Generation)  
**File phân tích**: `VietHistory.AI/Gemini/GeminiClient.cs`

---

## 🎯 1. CORE FEATURE OVERVIEW

### Tên Class: `GeminiStudyService`
**Implements**: `IAIStudyService`

### Mục đích:
- Trả lời câu hỏi lịch sử Việt Nam bằng AI (Gemini)
- Sử dụng RAG: Query MongoDB chunks trước
- Fallback sang Web (Wikipedia/Google) nếu không có data trong DB
- Tổng hợp context và gọi Gemini API

### Dependencies cần mock:
1. **HttpClient** `_http` - Call Gemini API + Web search
2. **MongoContext** `_ctx` - Query chunks và sources từ MongoDB
3. **GeminiOptions** `_opt` - Configuration (API key, model, temperature)

---

## 📊 2. FUNCTIONS CẦN TEST (9 Functions)

### 🔴 Priority P0 - Main Entry Point

#### **Function 1: `AskAsync(AiAskRequest req, CancellationToken ct)`**

**Main Functionality:**
- Main entry point cho AI Q&A
- Workflow:
  1. Validate API key/model
  2. Ensure MongoDB indexes
  3. Query MongoDB chunks (RAG)
  4. Fallback to web search if no MongoDB results
  5. Build prompt với context
  6. Call Gemini API
  7. Parse response và return AiAnswer

**Input Parameters:**
- `req: AiAskRequest` - Contains:
  - `Question: string?` - Câu hỏi của user
  - `Language: string?` - "vi" hoặc "en" (default: "vi")
  - `MaxContext: int` - Số chunks tối đa (default: 12, clamp 1-32)
- `ct: CancellationToken` - For cancellation

**Return Value:**
- `Task<AiAnswer>` - Contains:
  - `Answer: string` - Câu trả lời từ AI
  - `Model: string` - Model name (e.g. "gemini-2.5-flash")
  - `CostUsd: double?` - Nullable cost

**Edge Cases:**
1. Empty/null question → Should handle gracefully
2. `MaxContext = 0` → Should clamp to 1
3. `MaxContext = 100` → Should clamp to 32
4. `Language = null` → Should default to "vi"
5. MongoDB empty + Web fails → Still return answer from Gemini knowledge
6. Very long question (>1000 chars) → Should not crash

**Error Scenarios:**
1. Missing `ApiKey` → Throws `InvalidOperationException`
2. Missing `Model` → Throws `InvalidOperationException`
3. Gemini API timeout → Throws `TaskCanceledException`
4. Gemini API 429 (rate limit) → Throws `HttpRequestException`
5. Gemini API returns malformed JSON → Should handle parsing error
6. MongoDB connection error → Should fallback to web gracefully

**Dependencies to Mock:**
- `_http` (HttpClient) for Gemini API calls
- `_ctx.Chunks` (IMongoCollection<ChunkDoc>) for RAG
- `_ctx.Sources` (IMongoCollection<SourceDoc>) for source titles
- `_opt` (GeminiOptions) for config

---

### 🟡 Priority P1 - Helper Functions

#### **Function 2: `QueryTopChunksAsync(string question, string? sourceId, int limit, CancellationToken ct)`**

**Main Functionality:**
- Query MongoDB chunks using text search
- Fallback to regex search if text search fails
- Sort by text score

**Input Parameters:**
- `question: string` - Search query
- `sourceId: string?` - Optional filter by source
- `limit: int` - Max results
- `ct: CancellationToken`

**Return Value:**
- `Task<List<ChunkDoc>>` - List of matching chunks

**Edge Cases:**
1. Empty question → Returns empty list
2. No text index exists → Falls back to regex
3. Special regex characters in question → Should escape properly
4. `limit = 0` → Returns empty
5. `sourceId` filter với invalid ID → Returns empty

**Error Scenarios:**
1. MongoDB connection error → Catches and returns empty (fallback)
2. Regex compilation error → Should handle gracefully

**Dependencies to Mock:**
- `_ctx.Chunks.FindAsync()` - MongoDB text search
- `_ctx.Chunks.Find().Limit().ToListAsync()` - Regex fallback

---

#### **Function 3: `BuildChunkContextAsync(List<ChunkDoc> chunks, CancellationToken ct)`**

**Main Functionality:**
- Format chunks into readable context string
- Lookup source titles from MongoDB
- Format: "• [Title – Trang X-Y] snippet"

**Input Parameters:**
- `chunks: List<ChunkDoc>` - Chunks to format
- `ct: CancellationToken`

**Return Value:**
- `Task<string>` - Formatted context string (hoặc empty)

**Edge Cases:**
1. Empty chunks list → Returns empty string
2. Source not found in DB → Uses "Nguồn chưa rõ"
3. Multiple chunks from same source → Should list all
4. Very long content (>900 chars) → Truncates với "…"
5. Content có newlines → Converts to single line

**Error Scenarios:**
1. MongoDB error khi query sources → Should handle gracefully

**Dependencies to Mock:**
- `_ctx.Sources.Find().ToListAsync()` - Get source titles

---

#### **Function 4: `SearchWebAsync(string query, string? language, int max, CancellationToken ct)`**

**Main Functionality:**
- Search web for context nếu MongoDB empty
- Tries Google CSE first (nếu có API key)
- Falls back to Wikipedia (vi/en)
- Fetches Wikipedia summary for better snippets

**Input Parameters:**
- `query: string` - Search query
- `language: string?` - "vi" hoặc "en" (default: "vi")
- `max: int` - Max results (clamp 1-10)
- `ct: CancellationToken`

**Return Value:**
- `Task<List<WebSnippet>>` - List of web results (title, url, snippet)

**Edge Cases:**
1. Empty query → Returns empty list
2. `max = 0` → Clamps to 1
3. `max = 100` → Clamps to 10
4. `language = "en"` → Uses en.wikipedia.org
5. No Google CSE keys → Skips directly to Wikipedia
6. Wikipedia returns no results → Returns empty list

**Error Scenarios:**
1. Google CSE API error → Catches and falls back to Wikipedia
2. Wikipedia API error → Catches and returns empty
3. HTTP timeout → Catches and returns empty
4. Malformed JSON response → Catches and returns empty

**Dependencies to Mock:**
- `_http.GetAsync()` for Google CSE
- `_http.GetAsync()` for Wikipedia search
- `_http.GetAsync()` for Wikipedia summary

---

#### **Function 5: `EnsureChunkTextIndexOnce(CancellationToken ct)`**

**Main Functionality:**
- Tạo MongoDB indexes (text search + compound)
- Chỉ chạy 1 lần (static flag `_indexesEnsured`)
- Không throw error nếu không có quyền tạo index

**Input Parameters:**
- `ct: CancellationToken`

**Return Value:**
- `Task` (void)

**Edge Cases:**
1. Index đã tồn tại → MongoDB ignores silently
2. Multiple concurrent calls → Static flag prevents duplicate
3. No permission to create index → Catches error and continues

**Error Scenarios:**
1. MongoDB connection error → Catches và set flag = true anyway

**Dependencies to Mock:**
- `_ctx.Chunks.Indexes.CreateOneAsync()` - 2 calls (compound + text)

**Note**: Function này có thể skip testing vì:
- Side effect only (create indexes)
- Catches all errors
- Không affect business logic

---

### 🟢 Priority P2 - Pure Helper Functions

#### **Function 6: `OneLine(string s)`**

**Main Functionality:**
- Convert multi-line string to single line
- Replace `\r\n` and `\n` với space

**Input/Output:**
```csharp
Input: "Line 1\nLine 2\r\nLine 3"
Output: "Line 1 Line 2 Line 3"
```

**Edge Cases:**
1. Null input → Returns empty string
2. Empty string → Returns empty string
3. No newlines → Returns trimmed string
4. Multiple consecutive newlines → Multiple spaces (acceptable)

**Dependencies**: None (pure function)

**Note**: Có thể skip testing vì quá đơn giản

---

#### **Function 7: `Truncate(string s, int max)`**

**Main Functionality:**
- Truncate string nếu dài hơn max
- Thêm "…" nếu truncate

**Input/Output:**
```csharp
Input: ("Hello World", 5)
Output: "Hello…"

Input: ("Hi", 10)
Output: "Hi"
```

**Edge Cases:**
1. `max = 0` → Returns "…"
2. `max < 0` → Would throw error (không có validation)
3. String length = max → No truncation
4. String length = max + 1 → Truncates + adds "…"

**Dependencies**: None (pure function)

**Note**: Có thể skip testing vì quá đơn giản

---

#### **Function 8: `ExtractText(JsonElement root)`**

**Main Functionality:**
- Parse Gemini API JSON response
- Extract text from candidates[0].content.parts[].text
- Fallback to candidates[0].text nếu không có parts

**Input:**
```json
{
  "candidates": [
    {
      "content": {
        "parts": [
          { "text": "Answer text here" }
        ]
      }
    }
  ]
}
```

**Return Value:**
- `string?` - Extracted text hoặc null

**Edge Cases:**
1. No "candidates" property → Returns null
2. Empty candidates array → Returns null
3. No "content" property → Returns null
4. No "parts" array → Tries fallback "text" property
5. Empty parts array → Tries fallback "text" property
6. Multiple parts → Concatenates all text
7. Whitespace-only text → Returns null
8. Text with leading/trailing spaces → Trims

**Error Scenarios:**
1. Malformed JSON → Would have thrown before calling (caller responsibility)

**Dependencies**: None (takes JsonElement)

**Testing Priority**: HIGH - Complex parsing logic với nhiều edge cases

---

#### **Function 9: `JoinWebSnippets(List<WebSnippet> items)`**

**Main Functionality:**
- Format list of web snippets into string
- Format: "• [Title] Snippet (Nguồn: URL)"

**Input/Output:**
```csharp
Input: [
  { Title: "Wikipedia", Url: "https://...", Snippet: "Info here" }
]
Output: "• [Wikipedia] Info here (Nguồn: https://...)"
```

**Edge Cases:**
1. Empty list → Returns empty string
2. Title có brackets → Might look weird (acceptable)
3. Very long snippet → No truncation (handled by caller)

**Dependencies**: None (pure function)

**Note**: Có thể skip testing vì quá đơn giản

---

## 🎯 3. PRIORITIZATION FOR TESTING

### Must Test (Coverage Target: 80%+)
1. ✅ **AskAsync()** - 10-12 test cases
   - Happy paths (MongoDB context, web fallback, both)
   - Edge cases (empty question, maxContext bounds, language defaults)
   - Error scenarios (missing keys, API errors, timeouts)

2. ✅ **QueryTopChunksAsync()** - 4-5 test cases
   - Happy path (text search works)
   - Fallback to regex
   - Empty question
   - MongoDB errors

3. ✅ **BuildChunkContextAsync()** - 3-4 test cases
   - Format chunks correctly
   - Handle missing sources
   - Empty chunks list

4. ✅ **SearchWebAsync()** - 4-5 test cases
   - Wikipedia search works
   - Google CSE if configured
   - Handle errors gracefully
   - Language switching

5. ✅ **ExtractText()** - 4-5 test cases
   - Parse valid response
   - Handle missing properties
   - Multiple parts concatenation
   - Edge cases (empty, whitespace)

### Optional (Nice to Have)
6. ⚠️ **EnsureChunkTextIndexOnce()** - Skip hoặc 1-2 tests
7. ⚠️ **OneLine()** - Skip hoặc 1 test
8. ⚠️ **Truncate()** - Skip hoặc 1 test
9. ⚠️ **JoinWebSnippets()** - Skip hoặc 1 test

**Tổng ước tính**: 25-30 test cases → Chọn 20-25 để đạt yêu cầu thi

---

## 🧪 4. DEPENDENCIES SUMMARY

### External Dependencies (Need Mocking)
```csharp
// 1. HttpClient - For API calls
Mock<HttpMessageHandler> mockHttpHandler;
HttpClient mockHttpClient;

// 2. MongoContext - For database
// Issue: MongoContext là sealed class → Cần tạo interface IMongoContext
Mock<IMongoCollection<ChunkDoc>> mockChunks;
Mock<IMongoCollection<SourceDoc>> mockSources;

// 3. GeminiOptions - Configuration
GeminiOptions testOptions = new() {
    ApiKey = "test-key",
    Model = "gemini-2.5-flash",
    Temperature = 0.2
};
```

### Key Challenge: **MongoContext Mocking**
```csharp
// ❌ PROBLEM: MongoContext is sealed class
public sealed class MongoContext { ... }

// ✅ SOLUTION: Create interface
public interface IMongoContext {
    IMongoCollection<ChunkDoc> Chunks { get; }
    IMongoCollection<SourceDoc> Sources { get; }
}

// Then mock the interface
var mockContext = new Mock<IMongoContext>();
```

---

## 📈 5. EXPECTED COVERAGE

### Code Coverage Target: **80%+**

**Dự kiến coverage theo function:**
| Function | Test Cases | Coverage % |
|----------|-----------|------------|
| AskAsync() | 10-12 | 90%+ |
| QueryTopChunksAsync() | 4-5 | 85%+ |
| BuildChunkContextAsync() | 3-4 | 85%+ |
| SearchWebAsync() | 4-5 | 80%+ |
| ExtractText() | 4-5 | 95%+ |
| EnsureChunkTextIndexOnce() | 0-2 | 50% (optional) |
| Helpers (OneLine, etc) | 0-4 | 70% (optional) |
| **TOTAL** | **25-30** | **82-87%** |

---

## ✅ 6. NEXT STEPS (Giai đoạn 2)

Với phân tích này, Giai đoạn 2 sẽ:
1. Tạo bảng test cases matrix chi tiết (Given-When-Then)
2. Prioritize theo P0 → P1 → P2
3. Xác định mock data realistic cho Vietnamese history
4. Plan 20-25 test cases để đạt coverage >80%

**Thời gian hoàn thành Giai đoạn 1**: 15 phút ✅

---

**Generated by**: AI-Assisted Testing Workflow  
**Date**: 2025-10-24  
**Feature**: VietHistory AI - Historical Q&A

