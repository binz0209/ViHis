# TEXT_INGEST – Phase 2: Design (Result)

Date: 2025-01-XX

Artifacts Produced
- Test Matrix: `tests/TEXT_INGEST_TEST_PLAN.md` with 35 Given–When–Then cases across categories:
  - HappyPath (TC01-TC08), EdgeCase (TC09-TC16), ErrorHandling (TC17-TC20), Integration (TC21-TC35)
- Traits Standard (to apply in all tests):
  - `[Trait("Feature", "TEXT_INGEST")]`
  - `[Trait("Category", "HappyPath|EdgeCase|ErrorHandling|Integration")]`
  - `[Trait("Priority", "P0|P1|P2")]`
  - `[Trait("Integration", "Real")]` (for integration tests)

Global Prompt Applied (GWT Best Practice)
- Input: Function list from Phase 1 (14 functions/endpoints)
- Output: GWT cases below (selected highlights; full list in `tests/TEXT_INGEST_TEST_PLAN.md`).

GWT Cases (selected, per function)

1) IngestController.Preview
- Given valid PDF file; When POST /api/v1/ingest/preview; Then 200 with preview result (≤10 chunks). [HappyPath,P0]
- Given missing file; When POST preview; Then 400 BadRequest. [EdgeCase,P0]
- Given empty file; When POST preview; Then 400 BadRequest. [EdgeCase,P0]
- Given invalid PDF format; When POST preview; Then handled gracefully with error message. [ErrorHandling,P1]

2) IngestController.IngestAndSave
- Given valid PDF file; When POST /api/v1/ingest/pdf; Then 200 with sourceId, totalPages, totalChunks saved to MongoDB. [HappyPath,P0]
- Given missing file; When POST ingest; Then 400 BadRequest. [EdgeCase,P0]
- Given empty file; When POST ingest; Then 400 BadRequest. [EdgeCase,P0]
- Given missing title; When POST ingest; Then uses filename as title. [EdgeCase,P1]
- Given embedding API failure; When POST ingest; Then handles gracefully, saves chunks without embeddings. [ErrorHandling,P1]

3) IngestController.GetAllChunks
- Given no filter; When GET /api/v1/ingest/chunks; Then 200 with list of all chunks. [HappyPath,P1]
- Given sourceId filter; When GET chunks; Then 200 with filtered chunks. [HappyPath,P1]
- Given pagination (skip/take); When GET chunks; Then 200 with paginated results. [HappyPath,P1]
- Given nonexistent sourceId; When GET chunks; Then 200 with empty list. [EdgeCase,P2]

4) IngestController.GetAllSources
- Given sources exist; When GET /api/v1/ingest/sources; Then 200 with list sorted by Year desc, Title asc. [HappyPath,P1]
- Given pagination; When GET sources; Then 200 with paginated results. [EdgeCase,P1]

5) IngestController.GetSourceWithChunks
- Given valid sourceId; When GET /api/v1/ingest/source/{id}; Then 200 with source and chunks. [HappyPath,P1]
- Given nonexistent sourceId; When GET source; Then 404 NotFound. [EdgeCase,P1]
- Given invalid ObjectId format; When GET source; Then throws FormatException. [ErrorHandling,P2]

6) FallbackAIngestor.RunAsync
- Given valid PDF stream; When RunAsync; Then returns chunks with correct page ranges and tokens. [HappyPath,P0]
- Given empty PDF; When RunAsync; Then returns empty chunks list. [EdgeCase,P1]
- Given embedding failure for one chunk; When RunAsync; Then continues processing other chunks. [ErrorHandling,P1]
- Given custom ParserProfile; When RunAsync; Then uses profile settings. [Integration,P2]

7) PdfTextExtractor.ExtractPages
- Given valid PDF stream; When ExtractPages; Then returns PageText list with correct page numbers. [HappyPath,P0]
- Given invalid PDF format; When ExtractPages; Then throws exception. [ErrorHandling,P1]
- Given PDF without text layer; When ExtractPages; Then returns empty text per page. [EdgeCase,P1]

8) TextNormalizer.CleanRaw
- Given normal text with whitespace; When CleanRaw; Then removes extra whitespace. [HappyPath,P1]
- Given text with hyphen breaks; When CleanRaw; Then merges hyphenated words. [HappyPath,P1]
- Given text with spaced letters; When CleanRaw; Then condenses spaced letters. [HappyPath,P1]
- Given empty string; When CleanRaw; Then returns empty. [EdgeCase,P2]

9) TextNormalizer.CondenseSpacedLetters
- Given spaced letters (≥6 chars); When CondenseSpacedLetters; Then merges into word. [HappyPath,P2]
- Given normal text; When CondenseSpacedLetters; Then unchanged. [HappyPath,P2]
- Given short run (<6 chars); When CondenseSpacedLetters; Then does not merge. [EdgeCase,P2]

10) HeaderFooterDetector.Detect
- Given pages with common headers/footers; When Detect; Then returns detected headers/footers. [HappyPath,P1]
- Given pages with no common headers/footers; When Detect; Then returns empty sets. [HappyPath,P1]

11) HeaderFooterDetector.RemoveHeadersFooters
- Given pages with detected headers/footers; When RemoveHeadersFooters; Then removes and adds [Trang X] tag. [HappyPath,P1]
- Given pages with no matches; When RemoveHeadersFooters; Then unchanged except tag added. [HappyPath,P1]

12) SentenceTokenizer.SplitSentences
- Given normal text; When SplitSentences; Then splits correctly at sentence boundaries. [HappyPath,P2]
- Given text with abbreviations; When SplitSentences; Then protects abbreviations from splitting. [HappyPath,P2]
- Given empty text; When SplitSentences; Then returns empty list. [EdgeCase,P2]

13) ChunkPack.ApproxTokens
- Given normal text; When ApproxTokens; Then calculates correctly (length/4, min 1). [HappyPath,P2]
- Given empty string; When ApproxTokens; Then returns 1. [EdgeCase,P2]

14) ChunkPack.PackByTokens
- Given normal sentences; When PackByTokens; Then packs into chunks by target tokens with overlap. [HappyPath,P2]
- Given empty sentences; When PackByTokens; Then returns empty. [EdgeCase,P2]

Assertions & Idempotence
- Prefer structural patterns: count, non-empty strings, page ranges, token counts
- Avoid asserting specific text content (may vary)
- Integration tests use Real MongoDB and may need cleanup
- PDF parsing tests may require actual PDF files or skip if unavailable

Full Matrix (35 test cases)
- Unit Tests (21): TC01-TC21 in `TEXT_INGEST_UnitTests.cs`
  - TextNormalizer: 7 tests (TC01-TC08)
  - HeaderFooterDetector: 5 tests (TC09-TC13)
  - SentenceTokenizer: 3 tests (TC14-TC16)
  - ChunkPack: 6 tests (TC17-TC21)
- Integration Tests (14): TC01-TC15 in `TEXT_INGEST_IntegrationTests.cs`
  - GetAllChunks: 4 tests (TC01-TC04)
  - GetAllSources: 2 tests (TC05-TC06)
  - GetSourceWithChunks: 3 tests (TC07-TC09)
  - Preview: 2 tests (TC10-TC11)
  - IngestAndSave: 3 tests (TC13-TC15)

Exit Criteria
- Test matrix created with 35 GWT cases covering all functions from Phase 1
- Traits assigned correctly (Feature, Category, Priority, Integration)
- Test names follow TCxx_Given_When_Then format

