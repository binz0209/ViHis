# TEXT_INGEST – Phase 1: Analyze (Result)

Date: 2025-01-XX
Scope: Analyze end-to-end flow for PDF text ingestion feature, including extraction, normalization, chunking, embedding generation, and MongoDB storage.

Functions/Endpoints to test (per refined Phase 1 prompt)

1) IngestController.Preview(form: IngestUploadForm)
- Main: Preview PDF upload, extract first 10 chunks without saving to DB
- Inputs: IFormFile PDF, optional Title/Author/Year
- Returns: IngestPreviewResult { FileName, TotalPages, TotalChunks, Chunks[0..9] }
- Edge cases: File null/empty → 400; Invalid PDF format; Very large PDF (>100MB); PDF without text layer; Unicode/encoding issues
- Dependencies: IFallbackAIngestor, MongoDB (optional for preview mode)
- Logic branches: File validation → Extract pages → Normalize → Chunk → Return preview (max 10 chunks)
- Test type: Integration (Real MongoDB, Real PDF parsing)

2) IngestController.IngestAndSave(form: IngestUploadForm, ct)
- Main: Ingest PDF and save to MongoDB (Source + Chunks with embeddings)
- Inputs: IFormFile PDF, optional Title/Author/Year, CancellationToken
- Returns: IActionResult with sourceId, title, totalPages, totalChunks
- Edge cases: File null/empty → 400; Duplicate file upload; Gemini API rate limit (429); Network timeout during embedding; MongoDB write failures; Title missing → uses filename
- Dependencies: IFallbackAIngestor, ISourceRepository, IChunkRepository, Gemini Embedding API
- Logic branches: File validation → Create Source → Extract pages → Chunk → Generate embeddings (parallel) → Save to MongoDB → Update Source pages count
- Test type: Integration (Real MongoDB, Real Gemini API - may need retry handling)

3) IngestController.GetAllChunks(sourceId?, skip, take)
- Main: Retrieve all chunks (optionally filtered by sourceId) with pagination
- Inputs: Optional sourceId string, skip int (default 0), take int (default 100)
- Returns: List of ChunkDoc with count
- Edge cases: sourceId not found → Empty list; skip > total chunks → Empty list; take = 0 or negative; Very large take value
- Dependencies: IChunkRepository, MongoDB
- Logic branches: sourceId provided → Filter by sourceId; sourceId null/empty → Return all chunks; Pagination (skip/take)
- Test type: Integration (Real MongoDB)

4) IngestController.GetAllSources(skip, take)
- Main: List all ingested sources with pagination
- Inputs: skip int (default 0), take int (default 50)
- Returns: List of SourceDoc with count, sorted by Year desc then Title asc
- Edge cases: Empty collection; Pagination edge cases
- Dependencies: ISourceRepository, MongoDB
- Test type: Integration (Real MongoDB)

5) IngestController.GetSourceWithChunks(id)
- Main: Get single source with all its chunks
- Inputs: id string (sourceId)
- Returns: SourceDoc + list of ChunkDoc (sorted by ChunkIndex) or 404 if not found
- Edge cases: Invalid ObjectId format (MongoDB throws FormatException); Source with no chunks
- Dependencies: ISourceRepository, IChunkRepository
- Logic branches: Source exists → Return source + chunks; Source not found → 404 NotFound
- Test type: Integration (Real MongoDB)

6) FallbackAIngestor.RunAsync(pdfStream, sourceId, profile?)
- Main: Core ingest logic - extract PDF, normalize, chunk, generate embeddings, save
- Inputs: Stream (PDF), string sourceId, optional ParserProfile
- Returns: (IReadOnlyList<Chunk>, int TotalPages)
- Edge cases: Empty PDF; PDF with only images (no text); Very large PDF (memory); Embedding API failures (per chunk); Overlapping chunks calculation
- Dependencies: IPdfTextExtractor, IChunkRepository, GeminiOptions (for embedding)
- Logic branches: Extract pages → Merge short pages (<400 chars) → Detect headers/footers → Remove → Split sentences → Pack chunks (with overlap) → Generate embeddings (parallel) → Save to MongoDB
- Test type: Unit (Mock dependencies) + Integration (Real)

7) PdfTextExtractor.ExtractPages(pdfStream)
- Main: Extract text from PDF pages using PdfPig
- Inputs: Stream (PDF)
- Returns: IReadOnlyList<PageText> (PageNumber, Raw text)
- Edge cases: Invalid PDF format; Corrupted PDF; PDF without text layer (scanned images); Very large PDF (memory); Non-seekable stream
- Dependencies: PdfPig library, file system (stream)
- Logic branches: Copy stream to MemoryStream (seekable) → Open PDF → Extract text per page → Normalize CRLF→LF; Empty/null text → Return empty string
- Test type: Unit (Real PDF files as test data)

8) TextNormalizer.CleanRaw(text)
- Main: Normalize PDF text (hyphen breaks, spaced letters, whitespace)
- Inputs: string raw text
- Returns: string cleaned text
- Logic branches: CRLF → LF → Hyphen line breaks → Condense spaced letters → Soft line breaks → Multi-space → Trim
- Edge cases: Empty/null input; Very long text; Unicode handling
- Test type: Unit

9) TextNormalizer.CondenseSpacedLetters(input)
- Main: Merge spaced letters like "T e x t" → "Text" (PDF text tracking issue)
- Inputs: string input
- Returns: string condensed
- Logic branches: Detect runs of single-letter tokens (≥6) → Merge
- Edge cases: Empty input; No spaced letters; Edge of token boundaries
- Test type: Unit

10) HeaderFooterDetector.Detect(pages, headLines, footLines, freqThreshold)
- Main: Detect common header/footer lines across pages
- Inputs: IReadOnlyList<PageText>, headLines (default 2), footLines (default 2), freqThreshold (default 0.7)
- Returns: (HashSet<string> headers, HashSet<string> footers)
- Logic branches: Count line frequency at top/bottom → Filter by threshold
- Edge cases: Empty pages; All pages different; Very short pages
- Test type: Unit

11) HeaderFooterDetector.RemoveHeadersFooters(pages, headers, footers)
- Main: Remove detected headers/footers from pages
- Inputs: IReadOnlyList<PageText>, HashSet<string> headers, HashSet<string> footers
- Returns: IReadOnlyList<PageText> cleaned with [Trang X] tag
- Logic branches: Remove matching lines from top/bottom, add page tag
- Edge cases: Empty headers/footers; No matches
- Test type: Unit

12) SentenceTokenizer.SplitSentences(text, abbreviations)
- Main: Split text into sentences, protecting abbreviations
- Inputs: string text, string[] abbreviations
- Returns: List<string> sentences
- Logic branches: Protect abbreviations → Split by sentence delimiters → Restore abbreviations
- Edge cases: Empty text; No abbreviations; Abbreviations at sentence end
- Test type: Unit

13) ChunkPack.ApproxTokens(text)
- Main: Estimate token count (Vietnamese: ~4 chars/token)
- Inputs: string text
- Returns: int approximate tokens
- Logic branches: length / 4, minimum 1
- Edge cases: Empty string; Very long text
- Test type: Unit

14) ChunkPack.PackByTokens(sentences, targetTokens, overlapTokens)
- Main: Pack sentences into chunks by token target with overlap
- Inputs: IEnumerable<string> sentences, int targetTokens, int overlapTokens
- Returns: IEnumerable<(string text, int approxTokens)>
- Logic branches: Accumulate until target → Yield chunk → Keep overlap → Continue
- Edge cases: Empty sentences; Single long sentence; Very small target
- Test type: Unit

Prioritization
- High Priority (P0): IngestController.Preview, IngestController.IngestAndSave, FallbackAIngestor.RunAsync, PdfTextExtractor.ExtractPages
- Medium Priority (P1): IngestController.GetAllChunks, GetAllSources, GetSourceWithChunks, TextNormalizer methods, HeaderFooterDetector
- Low Priority (P2): SentenceTokenizer, ChunkPack (utility methods, tested indirectly via integration)

Assumptions & Invariants
- PDF files have text layer (not scanned images only)
- Gemini Embedding API may rate limit (429) - tests should handle gracefully
- MongoDB connection is available for integration tests
- ParserProfile defaults are sufficient for most cases

Risk Register
- PDF parsing: Large files (memory), corrupted files, missing text layer
- Embedding API: Rate limits (429), timeouts, network failures
- MongoDB: Write failures, connection issues
- Concurrency: Parallel embedding generation may fail partially
- Memory: Large PDFs may cause out-of-memory errors

Categories (mapping to test plan)
- HappyPath, EdgeCase, ErrorHandling, Integration, Performance

Exit Criteria
- Function list analyzed with inputs/returns/deps/branches/edge cases + ranking
- Coverage targets: ≥85% for IngestController, FallbackAIngestor, PdfTextExtractor
- Real API dependencies noted: Gemini Embedding API (may need retry/backoff)

