# EMBEDDINGS – Phase 1: Analyze (Result)

Date: 2025-01-XX

Scope: Analyze end-to-end flow for Embedding Management feature, including generating embeddings for chunks, checking status, and deleting embeddings.

## Functions/Endpoints to test (per refined Phase 1 prompt)

### 1) EmbeddingsController.GenerateAllEmbeddings(limit?, ct)
- **Main purpose**: Generate embeddings cho tất cả chunks chưa có embedding (hoặc limit chunks nếu limit provided)
- **Inputs**: Optional `limit` int (query parameter), `CancellationToken` ct
- **Returns**: `ActionResult` with { message, processed, errors, total, duration, averageTime }
- **Dependencies**: `IMongoContext.Chunks`, `EmbeddingService`, `ILogger`
- **Logic branches**:
  - EmbeddingService null → 400 BadRequest
  - Chunks without embedding found → Process chunks: Generate embedding → Update chunk → Continue
  - No chunks without embedding → 200 OK với message "All chunks already have embeddings"
  - Limit provided → Process only limit chunks
  - Rate limiting: 100ms delay between requests
  - Errors during processing → Count errors, continue with next chunk
  - Progress logging every 10 chunks
- **Edge cases**:
  - EmbeddingService null → 400 BadRequest
  - No chunks in database → 200 OK với processed=0
  - All chunks already have embeddings → 200 OK với processed=0
  - Limit=0 → No chunks processed
  - Limit provided → Only process limit chunks
  - Gemini API rate limit (429) → Error counted, continue
  - Gemini API timeout → Error counted, continue
  - MongoDB connection failure → 500 InternalServerError
  - Chunk content empty/null → EmbeddingService throws ArgumentException → Error counted
- **Test type**: Integration (Real MongoDB, Real Gemini Embedding API - may need retry handling)
- **Suggested Test Names**: TC01_GenerateAllEmbeddings_WithChunks_GeneratesEmbeddings, TC02_GenerateAllEmbeddings_NoChunks_ReturnsOk, TC03_GenerateAllEmbeddings_EmbeddingServiceNull_Returns400, TC04_GenerateAllEmbeddings_WithLimit_ProcessesLimitedChunks

### 2) EmbeddingsController.GetEmbeddingStatus(ct)
- **Main purpose**: Check embedding status (total chunks, with/without embeddings, percentage)
- **Inputs**: `CancellationToken` ct
- **Returns**: `ActionResult` with { total, withEmbedding, withoutEmbedding, percentage }
- **Dependencies**: `IMongoContext.Chunks`, `ILogger`
- **Logic branches**:
  - Total > 0 → Calculate percentage = (withEmbedding * 100.0 / total).ToString("F2")
  - Total = 0 → percentage = "0.00"
  - Calculate: withoutEmbedding = total - withEmbedding
- **Edge cases**:
  - Empty database → { total: 0, withEmbedding: 0, withoutEmbedding: 0, percentage: "0.00" }
  - All chunks have embeddings → percentage = "100.00"
  - No chunks have embeddings → percentage = "0.00"
  - Mixed (some have, some don't) → Calculate percentage correctly
  - MongoDB connection failure → 500 InternalServerError
- **Test type**: Integration (Real MongoDB)
- **Suggested Test Names**: TC01_GetEmbeddingStatus_WithMixedChunks_ReturnsStatus, TC02_GetEmbeddingStatus_EmptyDatabase_ReturnsZero, TC03_GetEmbeddingStatus_AllHaveEmbeddings_Returns100Percent

### 3) EmbeddingsController.DeleteAllEmbeddings(ct)
- **Main purpose**: Delete tất cả embeddings từ chunks (set embedding field to null)
- **Inputs**: `CancellationToken` ct
- **Returns**: `ActionResult` with { message, deleted } (deleted count)
- **Dependencies**: `IMongoContext.Chunks`, `ILogger`
- **Logic branches**:
  - Chunks exist → Update all chunks: Unset embedding field → Return deleted count
  - No chunks → deleted = 0
  - MongoDB UpdateManyAsync returns modified count
- **Edge cases**:
  - Empty database → deleted = 0
  - All chunks have embeddings → deleted = total count
  - Some chunks have embeddings → deleted = count with embeddings
  - No chunks have embeddings → deleted = 0
  - MongoDB connection failure → 500 InternalServerError
- **Test type**: Integration (Real MongoDB)
- **Suggested Test Names**: TC01_DeleteAllEmbeddings_WithEmbeddings_DeletesAll, TC02_DeleteAllEmbeddings_NoEmbeddings_ReturnsZero, TC03_DeleteAllEmbeddings_EmptyDatabase_ReturnsZero

### 4) EmbeddingService.GenerateEmbeddingAsync(text, ct)
- **Main purpose**: Generate embedding cho document (RETRIEVAL_DOCUMENT task_type)
- **Inputs**: `text` string, `CancellationToken` ct
- **Returns**: `Task<List<float>>` (768-dimension vector)
- **Dependencies**: `HttpClient`, Gemini Embedding API (`text-embedding-004`)
- **Logic branches**:
  - Text null/empty → Throw ArgumentException
  - Valid text → POST to Gemini API → Parse response → Return List<float>
  - API response parsing: Check "embedding" → "values" → Convert to List<float>
  - API failure → Throw InvalidOperationException
- **Edge cases**:
  - Text null/empty → ArgumentException
  - Gemini API 429 rate limit → HttpRequestException
  - Gemini API timeout → HttpRequestException
  - Invalid API response format → InvalidOperationException
  - Network failure → HttpRequestException
- **Test type**: Integration (Real Gemini Embedding API) - Có thể bị rate limit
- **Suggested Test Names**: TC01_GenerateEmbeddingAsync_ValidText_ReturnsVector, TC02_GenerateEmbeddingAsync_EmptyText_ThrowsArgumentException, TC03_GenerateEmbeddingAsync_NullText_ThrowsArgumentException

### 5) EmbeddingService.GenerateQueryEmbeddingAsync(text, ct)
- **Main purpose**: Generate embedding cho query (RETRIEVAL_QUERY task_type)
- **Inputs**: `text` string, `CancellationToken` ct
- **Returns**: `Task<List<float>>` (768-dimension vector)
- **Dependencies**: `HttpClient`, Gemini Embedding API (`text-embedding-004`)
- **Logic branches**:
  - Text null/empty → Throw ArgumentException
  - Valid text → POST to Gemini API (task_type: RETRIEVAL_QUERY) → Parse response → Return List<float>
  - API response parsing: Check "embedding" → "values" → Convert to List<float>
  - API failure → Throw InvalidOperationException
- **Edge cases**:
  - Text null/empty → ArgumentException
  - Gemini API 429 rate limit → HttpRequestException
  - Gemini API timeout → HttpRequestException
  - Invalid API response format → InvalidOperationException
  - Network failure → HttpRequestException
- **Test type**: Integration (Real Gemini Embedding API) - Có thể bị rate limit
- **Suggested Test Names**: TC01_GenerateQueryEmbeddingAsync_ValidText_ReturnsVector, TC02_GenerateQueryEmbeddingAsync_EmptyText_ThrowsArgumentException

## Prioritization

### High Priority (P0)
1. **GenerateAllEmbeddings** - Core functionality, generate embeddings cho chunks
2. **GetEmbeddingStatus** - Admin tool, check embedding status

### Medium Priority (P1)
3. **DeleteAllEmbeddings** - Admin tool, delete/regenerate embeddings
4. **EmbeddingService.GenerateEmbeddingAsync** - Service layer, core functionality
5. **EmbeddingService.GenerateQueryEmbeddingAsync** - Service layer, used by KWideRetriever

### Low Priority (P2)
- Performance tests (generate embeddings cho large batch)
- Concurrency tests (multiple requests generating embeddings)

## Risk Register

### High Risk
- ⚠️ **Gemini Embedding API rate limit (429)**: Similar to AI_QA, có thể bị rate limit khi generate nhiều embeddings
- **Mitigation**: 
  - Rate limiting trong code (100ms delay between requests)
  - Error handling: Count errors, continue with next chunk
  - Test với limit nhỏ trước (limit=5 hoặc limit=10)
  - Accept failed tests do rate limit (mark as "current behavior")

### Medium Risk
- ⚠️ **Timeout**: Gemini API timeout (60s) có thể xảy ra với large batches
- **Mitigation**: Error handling trong GenerateAllEmbeddings

### Low Risk
- ⚠️ **MongoDB connection failure**: Rare, but should handle gracefully
- **Mitigation**: Try-catch trong controller methods

## Assumptions

1. ✅ **Gemini Embedding API available**: Tests assume Gemini Embedding API is accessible
2. ✅ **MongoDB connection available**: Tests assume MongoDB is accessible
3. ✅ **EmbeddingService optional**: EmbeddingsController có thể work với EmbeddingService null (returns 400)
4. ✅ **Rate limiting**: Code có rate limiting (100ms delay) nhưng vẫn có thể bị 429
5. ✅ **Test database**: Tests dùng `vihis_test` database (same as other integration tests)

## Acceptance (Phase 1)

- ✅ 5 functions/endpoints identified với đầy đủ inputs/returns/deps/branches/edge cases
- ✅ Prioritization: High (2), Medium (3), Low (1)
- ✅ Risk register documented: Rate limit, timeout, MongoDB failure
- ✅ Assumptions documented: API availability, database, rate limiting
- ✅ Test types identified: Integration (Real MongoDB, Real Gemini Embedding API)

## Coverage Target

- **EmbeddingsController**: ≥85% coverage
- **EmbeddingService**: ≥85% coverage (nếu có unit tests) hoặc Integration coverage only
- **Total estimated test cases**: ~15-20 tests (Integration: 12-15, Unit: 3-5)

