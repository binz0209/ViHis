# 📋 GIAI ĐOẠN 2: THIẾT KẾ TEST CASES

**Thời gian**: 20 phút  
**Feature**: GeminiStudyService.AskAsync()  
**Total Test Cases**: 25 (20 Unit + 5 Integration)  
**Format**: Given-When-Then

---

## 🎯 TEST STRATEGY (REVISED - UNIT TEST FOCUS)

### Unit Tests (20 tests) - Mock Dependencies ✅ PRIORITY
- Focus: Business logic isolation
- Mock: MongoDB, HttpClient (Gemini API)
- Fast execution, no external dependencies
- **Coverage all edge cases & error scenarios**

### Integration Tests (5 tests) - Real MongoDB Atlas + Gemini API
- Focus: Critical end-to-end validation only
- Đáp ứng yêu cầu thi (feature phức tạp cần integration test)
- Verify actual behavior với real API

**Total: 25 tests**
**Coverage Target**: >80%

---

## 📊 UNIT TESTS - TEST CASES MATRIX (20 tests)

### 🟢 HAPPY PATH - MongoDB Context (3 tests)

| ID | Priority | Test Name | Given | When | Then | Mock Required |
|----|----------|-----------|-------|------|------|---------------|
| **TC01** | P0 | `AskAsync_WithMongoDBContext_ReturnsValidAnswer` | - MongoDB có 3 chunks về "Trận Bạch Đằng 938"<br>- Chunks chứa: "Ngô Quyền đánh tan quân Nam Hán"<br>- Source: "Đại Việt Sử Ký Toàn Thư" | Call `AskAsync(new AiAskRequest("Trận Bạch Đằng xảy ra năm nào?", "vi", 5))` | - Returns `AiAnswer`<br>- `Answer` contains "938" hoặc "Ngô Quyền"<br>- `Model` = "gemini-2.5-flash"<br>- No exception | - Mock `_ctx.Chunks.FindAsync()` → return chunks<br>- Mock `_ctx.Sources.Find()` → return source info<br>- Mock `HttpClient.PostAsync()` → Gemini response |
| **TC02** | P0 | `AskAsync_WithEmptyMongoDB_FallsBackToWeb` | - MongoDB returns empty list<br>- Wikipedia có article "Lý Thường Kiệt" | Call `AskAsync(new AiAskRequest("Lý Thường Kiệt là ai?", "vi", 5))` | - Calls Wikipedia API (verify `HttpClient.GetAsync` called)<br>- Returns valid answer từ web context<br>- `Answer` not empty | - Mock `_ctx.Chunks.FindAsync()` → empty<br>- Mock `HttpClient.GetAsync()` → Wikipedia search results<br>- Mock `HttpClient.GetAsync()` → Wikipedia summary<br>- Mock `HttpClient.PostAsync()` → Gemini response |
| **TC03** | P1 | `AskAsync_WithBothMongoAndWeb_UsesMongoFirst` | - MongoDB có 2 chunks về "Trần Hưng Đạo"<br>- Wikipedia cũng có data | Call `AskAsync(new AiAskRequest("Trần Hưng Đạo là ai?", "vi", 5))` | - Uses MongoDB context (không call Wikipedia)<br>- Returns answer based on MongoDB chunks<br>- `HttpClient.GetAsync` NOT called for Wikipedia | - Mock `_ctx.Chunks.FindAsync()` → return chunks<br>- Mock `_ctx.Sources.Find()` → return source<br>- Mock `HttpClient.PostAsync()` → Gemini response<br>- Verify Wikipedia NOT called |

---

### 🟡 EDGE CASES - Input Validation (5 tests)

| ID | Priority | Test Name | Given | When | Then | Mock Required |
|----|----------|-----------|-------|------|------|---------------|
| **TC04** | P1 | `AskAsync_WithEmptyQuestion_ReturnsGracefully` | - Question = ""<br>- MongoDB và API configured | Call `AskAsync(new AiAskRequest("", "vi", 5))` | - No exception thrown<br>- Returns valid `AiAnswer`<br>- Answer có message phù hợp hoặc generic response | - Mock `_ctx.Chunks.FindAsync()` → empty (vì question empty)<br>- Mock `HttpClient.PostAsync()` → Gemini response |
| **TC05** | P1 | `AskAsync_MaxContextZero_ClampsToOne` | - MaxContext = 0<br>- MongoDB có nhiều chunks | Call `AskAsync(new AiAskRequest("Test", "vi", 0))` | - Internally clamps MaxContext to 1<br>- Calls MongoDB với limit = 1<br>- Returns valid answer | - Mock `_ctx.Chunks.FindAsync()` with `FindOptions.Limit = 1`<br>- Verify limit parameter = 1<br>- Mock Gemini response |
| **TC06** | P1 | `AskAsync_MaxContext100_ClampsTo32` | - MaxContext = 100<br>- MongoDB có nhiều chunks | Call `AskAsync(new AiAskRequest("Test", "vi", 100))` | - Clamps MaxContext to 32<br>- Calls MongoDB với limit = 32<br>- Returns valid answer | - Mock `_ctx.Chunks.FindAsync()` with `Limit = 32`<br>- Verify limit parameter = 32<br>- Mock Gemini response |
| **TC07** | P1 | `AskAsync_NullLanguage_DefaultsToVietnamese` | - Language = null | Call `AskAsync(new AiAskRequest("Test", null, 5))` | - Uses "vi" as default language<br>- System prompt contains "vi"<br>- Returns answer | - Mock MongoDB + Gemini<br>- Capture request body to Gemini<br>- Verify "vi" in system prompt |
| **TC08** | P2 | `AskAsync_SpecialCharactersInQuestion_HandlesCorrectly` | - Question = "Trận Bạch Đằng (938) & chiến thắng 'vĩ đại'!" | Call `AskAsync(new AiAskRequest([question với special chars], "vi", 5))` | - No exception<br>- MongoDB query works (regex escapes chars)<br>- Returns valid answer | - Mock MongoDB query (verify regex escape)<br>- Mock Gemini response |

---

### 🔴 ERROR SCENARIOS - Exception Handling (7 tests)

| ID | Priority | Test Name | Given | When | Then | Mock Required |
|----|----------|-----------|-------|------|------|---------------|
| **TC09** | P0 | `AskAsync_MissingAPIKey_ThrowsInvalidOperationException` | - `GeminiOptions.ApiKey = ""`<br>- Request valid | Call `AskAsync(valid request)` | - Throws `InvalidOperationException`<br>- Message: "Missing Gemini API key..." | - No mocks needed (validation before API call) |
| **TC10** | P0 | `AskAsync_MissingModel_ThrowsInvalidOperationException` | - `GeminiOptions.Model = ""`<br>- Request valid | Call `AskAsync(valid request)` | - Throws `InvalidOperationException`<br>- Message: "Missing Gemini model..." | - No mocks needed |
| **TC11** | P1 | `AskAsync_GeminiAPITimeout_ThrowsTaskCanceledException` | - MongoDB returns chunks<br>- Gemini API times out | Call `AskAsync(valid request)` | - Throws `TaskCanceledException` hoặc `HttpRequestException`<br>- Error propagates to caller | - Mock `_ctx.Chunks` → return chunks<br>- Mock `HttpClient.PostAsync()` → throw `TaskCanceledException` |
| **TC12** | P1 | `AskAsync_GeminiAPI429_ThrowsHttpRequestException` | - MongoDB returns chunks<br>- Gemini returns 429 (rate limit) | Call `AskAsync(valid request)` | - Throws `HttpRequestException`<br>- Status code = 429 | - Mock MongoDB<br>- Mock `HttpClient.PostAsync()` → return 429 response |
| **TC13** | P1 | `AskAsync_GeminiReturnsEmptyCandidates_ReturnsFallbackMessage` | - MongoDB returns chunks<br>- Gemini returns `{"candidates": []}` | Call `AskAsync(valid request)` | - Returns `AiAnswer`<br>- `Answer = "(Không nhận được câu trả lời từ mô hình.)"` | - Mock MongoDB<br>- Mock Gemini → return JSON with empty candidates |
| **TC14** | P0 | `AskAsync_MongoDBConnectionError_FallsBackToWebGracefully` | - MongoDB throws `MongoException`<br>- Wikipedia available | Call `AskAsync(valid request)` | - Catches MongoDB error<br>- Falls back to Wikipedia<br>- Returns valid answer from web | - Mock `_ctx.Chunks.FindAsync()` → throw `MongoException`<br>- Mock Wikipedia API → return results<br>- Mock Gemini → return answer |
| **TC15** | P2 | `AskAsync_WikipediaFails_GeminiAnswersWithoutContext` | - MongoDB empty<br>- Wikipedia throws exception<br>- Gemini API works | Call `AskAsync(valid request)` | - Catches Wikipedia error<br>- Calls Gemini without web context<br>- Returns answer từ Gemini knowledge | - Mock MongoDB → empty<br>- Mock Wikipedia → throw exception<br>- Mock Gemini → return answer |

---

### 🟣 HELPER FUNCTIONS - Indirect Testing (5 tests)

| ID | Priority | Test Name | Given | When | Then | Mock Required |
|----|----------|-----------|-------|------|------|---------------|
| **TC16** | P1 | `QueryTopChunksAsync_TextSearch_ReturnsMatchingChunks` | - MongoDB text index exists<br>- Question = "Bạch Đằng" | Call `QueryTopChunksAsync("Bạch Đằng", null, 5, ct)` | - Returns list of ChunkDoc<br>- Sorted by text score<br>- Max 5 results | - Mock `_ctx.Chunks.FindAsync()` with text filter<br>- Return sorted chunks |
| **TC17** | P1 | `QueryTopChunksAsync_TextSearchFails_FallsBackToRegex` | - Text search throws exception<br>- Regex search works | Call `QueryTopChunksAsync("test", null, 3, ct)` | - Catches text search error<br>- Falls back to regex search<br>- Returns results from regex | - Mock `FindAsync()` → throw exception<br>- Mock `Find().Limit().ToListAsync()` → return results |
| **TC18** | P1 | `BuildChunkContextAsync_FormatsChunksCorrectly` | - 2 chunks from different sources<br>- Sources có titles | Call `BuildChunkContextAsync(chunks, ct)` | - Returns formatted string<br>- Format: "• [Title – Trang X] content"<br>- Each chunk on separate line | - Mock `_ctx.Sources.Find().ToListAsync()` → return sources |
| **TC19** | P2 | `ExtractText_ValidGeminiResponse_ReturnsText` | - Valid JSON: `{"candidates": [{"content": {"parts": [{"text": "Answer"}]}}]}` | Call `ExtractText(jsonElement)` | - Returns "Answer"<br>- Trimmed whitespace<br>- No null | - No mocks (pure function with JsonElement) |
| **TC20** | P2 | `ExtractText_MissingCandidates_ReturnsNull` | - JSON: `{"error": "something"}` (no candidates) | Call `ExtractText(jsonElement)` | - Returns null<br>- No exception thrown | - No mocks |

---

## 🌐 INTEGRATION TESTS - TEST CASES MATRIX (5 tests - Critical Only)

### Real MongoDB Atlas + Real Gemini API

| ID | Priority | Test Name | Given | When | Then | Setup Required |
|----|----------|-----------|-------|------|------|----------------|
| **TC21** | P0 | `RealAPI_VietnameseHistoryQuestion_ReturnsValidAnswer` | - Real Atlas connection<br>- Real Gemini API key<br>- Question: "Trần Hưng Đạo là ai?" | Call `AskAsync()` với real dependencies | - Returns `AiAnswer`<br>- Answer contains relevant info<br>- Response time < 10 seconds<br>- No exception | - ConnectionString từ credentials<br>- Real `MongoContext`<br>- Real `HttpClient`<br>- Real `GeminiOptions` |
| **TC22** | P0 | `RealAPI_QuestionNotInDatabase_FallsBackToWeb` | - Real setup<br>- Question về topic không có trong DB | Call `AskAsync()` | - Successfully falls back to Wikipedia/Google<br>- Returns valid answer<br>- Verifies web search was used | Same as TC21 + log verification |
| **TC23** | P0 | `RealAPI_ResponseTime_UnderTenSeconds` | - Real setup<br>- Standard question | Measure execution time of `AskAsync()` | - Total time < 10 seconds<br>- Returns valid answer<br>- Performance acceptable | Same as TC21 + Stopwatch |
| **TC24** | P1 | `RealAPI_ConcurrentRequests_AllSucceed` | - Real setup<br>- 3 different questions in parallel | Call `AskAsync()` 3 times concurrently | - All 3 requests succeed<br>- No race conditions<br>- Each returns unique answer | Same as TC21 + `Task.WhenAll()` |
| **TC25** | P1 | `RealAPI_EnglishLanguage_ReturnsEnglishAnswer` | - Real setup<br>- Question: "Who was Tran Hung Dao?"<br>- Language = "en" | Call `AskAsync()` | - Returns answer in English<br>- Answer relevant to Vietnamese history<br>- No crash | Same as TC21 với language="en" |

---

## 📈 COVERAGE ANALYSIS

### Expected Coverage by Function:

| Function | Unit Tests | Integration Tests | Total | Coverage % |
|----------|-----------|-------------------|-------|------------|
| `AskAsync()` | 15 | 5 | 20 | 95% |
| `QueryTopChunksAsync()` | 2 (direct) + 5 (indirect) | 3 (indirect) | 10 | 90% |
| `BuildChunkContextAsync()` | 1 (direct) + 4 (indirect) | 2 (indirect) | 7 | 85% |
| `SearchWebAsync()` | 3 (indirect) | 1 (indirect) | 4 | 80% |
| `ExtractText()` | 2 (direct) + 2 (indirect) | 1 (indirect) | 5 | 90% |
| `EnsureChunkTextIndexOnce()` | 0 | 1 (indirect) | 1 | 60% |
| Helper functions (OneLine, Truncate, etc) | 0 | 0 | 0 | 0% (skipped - too simple) |
| **TOTAL** | **20** | **5** | **25** | **~85%** ✅ |

---

## 🎯 MOCK DATA REFERENCE

### Sample ChunkDoc for Mocking:
```csharp
new ChunkDoc
{
    Id = "chunk-001",
    SourceId = "source-dvsktth",
    Content = "Năm 938, Ngô Quyền đánh tan quân Nam Hán tại sông Bạch Đằng, chấm dứt 1000 năm Bắc thuộc.",
    PageFrom = 15,
    PageTo = 15,
    ChunkIndex = 0
}
```

### Sample SourceDoc for Mocking:
```csharp
new SourceDoc
{
    Id = "source-dvsktth",
    Title = "Đại Việt Sử Ký Toàn Thư",
    FileName = "dai-viet-su-ky.pdf",
    Pages = 500
}
```

### Sample Gemini API Response for Mocking:
```json
{
  "candidates": [
    {
      "content": {
        "parts": [
          {
            "text": "Trận Bạch Đằng xảy ra năm 938, khi Ngô Quyền đánh tan quân Nam Hán trên sông Bạch Đằng."
          }
        ]
      }
    }
  ]
}
```

### Sample Wikipedia Response for Mocking:
```json
[
  "Lý Thường Kiệt",
  ["Lý Thường Kiệt"],
  ["Tướng quân thời Lý"],
  ["https://vi.wikipedia.org/wiki/Lý_Thường_Kiệt"]
]
```

---

## ✅ DELIVERABLE CHECKLIST

- [x] 25 test cases designed (**20 unit + 5 integration**) ✅ UNIT TEST FOCUS
- [x] Given-When-Then format
- [x] Priority assigned (P0, P1, P2)
- [x] Mock requirements identified
- [x] Realistic Vietnamese history data
- [x] Coverage analysis shows **~85%** ✅
- [x] Integration test setup requirements documented
- [x] Test cases cover: AskAsync, QueryTopChunks, BuildChunkContext, ExtractText

---

## 🚀 NEXT STEP: Giai đoạn 3

**Ready to generate test code!**
- Thời gian: 75 phút
- Output: 2 test files (.cs)
- Framework: xUnit + Moq + FluentAssertions
- Real credentials: Ready for integration tests

**Status**: ✅ Phase 2 COMPLETE (20 phút)

---

**Generated by**: AI-Assisted Testing Workflow  
**Date**: 2025-10-24  
**Strategy**: **UNIT TEST FOCUS** (20 unit + 5 integration)  
**Total Test Cases**: 25 (meets >20 requirement) ✅  
**Expected Coverage**: ~85% ✅

