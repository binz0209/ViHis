# EMBEDDINGS – Phase 2: Design (Result)

Date: 2025-01-XX

Artifacts Produced
- Test Matrix: `tests/EMBEDDINGS_TEST_PLAN.md` with ~15 Given–When–Then cases across categories:
  - HappyPath (TC01-TC06), EdgeCase (TC07-TC10), ErrorHandling (TC11-TC14), Integration (TC15-TC18)
- Traits Standard (to apply in all tests):
  - `[Trait("Feature", "EMBEDDINGS")]`
  - `[Trait("Category", "HappyPath|EdgeCase|ErrorHandling|Integration")]`
  - `[Trait("Priority", "P0|P1|P2")]`
  - `[Trait("Integration", "Real")]` (for integration tests)

## Global Prompt Applied (GWT Best Practice)

Input: Function list from Phase 1 (5 functions/endpoints)
Output: GWT cases below (selected highlights; full list in `tests/EMBEDDINGS_TEST_PLAN.md`).

## GWT Cases (selected, per function)

### 1) EmbeddingsController.GenerateAllEmbeddings

| Category | TC Name | Given | When | Then | Traits |
|----------|---------|-------|------|------|--------|
| HappyPath | TC01_GenerateAllEmbeddings_WithChunks_GeneratesEmbeddings | chunks exist without embeddings, EmbeddingService configured | POST /api/v1/admin/embeddings/generate-all | 200 + processed count, embeddings saved | Feature=EMBEDDINGS; P0; Integration=Real |
| HappyPath | TC02_GenerateAllEmbeddings_WithLimit_ProcessesLimitedChunks | chunks exist, limit=5 provided | POST /api/v1/admin/embeddings/generate-all?limit=5 | 200 + processed=5 | Feature=EMBEDDINGS; P0; Integration=Real |
| EdgeCase | TC03_GenerateAllEmbeddings_NoChunks_ReturnsOk | empty database | POST /api/v1/admin/embeddings/generate-all | 200 + processed=0 | Feature=EMBEDDINGS; P1; Integration=Real |
| EdgeCase | TC04_GenerateAllEmbeddings_AllHaveEmbeddings_ReturnsOk | all chunks already have embeddings | POST /api/v1/admin/embeddings/generate-all | 200 + message "All chunks already have embeddings" | Feature=EMBEDDINGS; P1; Integration=Real |
| EdgeCase | TC05_GenerateAllEmbeddings_WithLimitZero_ProcessesZero | limit=0 provided | POST /api/v1/admin/embeddings/generate-all?limit=0 | 200 + processed=0 | Feature=EMBEDDINGS; P2; Integration=Real |
| ErrorHandling | TC06_GenerateAllEmbeddings_EmbeddingServiceNull_Returns400 | EmbeddingService is null | POST /api/v1/admin/embeddings/generate-all | 400 BadRequest + error message | Feature=EMBEDDINGS; P0; Integration=Real |
| ErrorHandling | TC07_GenerateAllEmbeddings_PartialFailures_CountsErrors | some chunks fail (API errors) | POST /api/v1/admin/embeddings/generate-all | 200 + processed > 0, errors > 0 | Feature=EMBEDDINGS; P1; Integration=Real |

### 2) EmbeddingsController.GetEmbeddingStatus

| Category | TC Name | Given | When | Then | Traits |
|----------|---------|-------|------|------|--------|
| HappyPath | TC08_GetEmbeddingStatus_WithMixedChunks_ReturnsStatus | chunks with/without embeddings | GET /api/v1/admin/embeddings/status | 200 + total, withEmbedding, withoutEmbedding, percentage | Feature=EMBEDDINGS; P0; Integration=Real |
| EdgeCase | TC09_GetEmbeddingStatus_EmptyDatabase_ReturnsZero | empty database | GET /api/v1/admin/embeddings/status | 200 + total=0, percentage="0.00" | Feature=EMBEDDINGS; P1; Integration=Real |
| EdgeCase | TC10_GetEmbeddingStatus_AllHaveEmbeddings_Returns100Percent | all chunks have embeddings | GET /api/v1/admin/embeddings/status | 200 + percentage="100.00" | Feature=EMBEDDINGS; P1; Integration=Real |
| EdgeCase | TC11_GetEmbeddingStatus_NoEmbeddings_Returns0Percent | no chunks have embeddings | GET /api/v1/admin/embeddings/status | 200 + percentage="0.00" | Feature=EMBEDDINGS; P1; Integration=Real |

### 3) EmbeddingsController.DeleteAllEmbeddings

| Category | TC Name | Given | When | Then | Traits |
|----------|---------|-------|------|------|--------|
| HappyPath | TC12_DeleteAllEmbeddings_WithEmbeddings_DeletesAll | chunks with embeddings | DELETE /api/v1/admin/embeddings/all | 200 + deleted count > 0 | Feature=EMBEDDINGS; P1; Integration=Real |
| EdgeCase | TC13_DeleteAllEmbeddings_NoEmbeddings_ReturnsZero | no chunks have embeddings | DELETE /api/v1/admin/embeddings/all | 200 + deleted=0 | Feature=EMBEDDINGS; P1; Integration=Real |
| EdgeCase | TC14_DeleteAllEmbeddings_EmptyDatabase_ReturnsZero | empty database | DELETE /api/v1/admin/embeddings/all | 200 + deleted=0 | Feature=EMBEDDINGS; P1; Integration=Real |

### 4) EmbeddingService.GenerateEmbeddingAsync

| Category | TC Name | Given | When | Then | Traits |
|----------|---------|-------|------|------|--------|
| HappyPath | TC15_GenerateEmbeddingAsync_ValidText_ReturnsVector | valid text, Gemini API available | GenerateEmbeddingAsync("test text") | Returns List<float> with 768 dimensions | Feature=EMBEDDINGS; P1; Integration=Real |
| ErrorHandling | TC16_GenerateEmbeddingAsync_EmptyText_ThrowsArgumentException | empty text | GenerateEmbeddingAsync("") | Throws ArgumentException | Feature=EMBEDDINGS; P0; Integration=Real |
| ErrorHandling | TC17_GenerateEmbeddingAsync_NullText_ThrowsArgumentException | null text | GenerateEmbeddingAsync(null) | Throws ArgumentException | Feature=EMBEDDINGS; P0; Integration=Real |

### 5) EmbeddingService.GenerateQueryEmbeddingAsync

| Category | TC Name | Given | When | Then | Traits |
|----------|---------|-------|------|------|--------|
| HappyPath | TC18_GenerateQueryEmbeddingAsync_ValidText_ReturnsVector | valid text, Gemini API available | GenerateQueryEmbeddingAsync("test query") | Returns List<float> with 768 dimensions | Feature=EMBEDDINGS; P1; Integration=Real |
| ErrorHandling | TC19_GenerateQueryEmbeddingAsync_EmptyText_ThrowsArgumentException | empty text | GenerateQueryEmbeddingAsync("") | Throws ArgumentException | Feature=EMBEDDINGS; P0; Integration=Real |

## Test Matrix Summary

**Total Test Cases**: ~18-20 tests

**By Category**:
- HappyPath: 6 tests
- EdgeCase: 8 tests
- ErrorHandling: 4 tests
- Integration: 0 (all are Integration tests)

**By Priority**:
- P0 (High): 5 tests
- P1 (Medium): 11 tests
- P2 (Low): 2 tests

**By Feature**:
- EmbeddingsController: 14 tests
- EmbeddingService: 4 tests

## Acceptance (Phase 2)

- ✅ Ma trận đủ phủ các nhánh chính/ngoại lệ
- ✅ Tên TCxx rõ ràng (Given-When-Then format)
- ✅ Traits đầy đủ (Feature, Category, Priority, Integration)
- ✅ Coverage: Controller (3 endpoints), Service (2 methods)

