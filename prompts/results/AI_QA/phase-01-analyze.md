# AI_QA – Phase 1: Analyze (Result)

Date: 2025-10-30
Scope: Analyze end-to-end flow for `GeminiStudyService.AskAsync` (AI Q&A) with real MongoDB Atlas + Gemini + web fallback.

Functions to test (per refined Phase 1 prompt)

1) GeminiStudyService.AskAsync(request: AiAskRequest) → AiAnswer
- Main: Orchestrate RAG → LLM → fallback, return synthesized answer
- Inputs: question:string (non-empty), language?:string|null, maxContext?:int (clamped)
- Returns: AiAnswer { answer:string, model:string, costUsd?:number|null }
- Edge cases: empty/whitespace question; very long question; invalid/null language; maxContext (0/1000); empty candidates; timeouts; 429; Mongo unavailable; repeated concurrent calls
- Dependencies: IMongoContext (chunks), HTTP client to Gemini, Wikipedia/Google web search

2) AI Controller endpoint POST /api/v1/ai/ask (AiController.Ask)
- Main: HTTP surface for AskAsync
- Inputs: AiAskRequest JSON (body), headers (X-Machine-Id optional)
- Returns: 200 with AiAnswer; 4xx for validation; 5xx for internal errors
- Edge cases: invalid JSON; missing fields; extremely large payload; rate limiting behavior
- Dependencies: IAIStudyService (GeminiStudyService), ASP.NET model binding/validation

3) Mongo query layer used by RAG (via IMongoContext and any query helpers)
- Main: Retrieve top-N relevant chunks by text criteria
- Inputs: request.question, maxContext; internal filters/sorts
- Returns: collection of chunks (0..N)
- Edge cases: 0 results; very large datasets; slow network; malformed records
- Dependencies: MongoDB Atlas driver; network latency

4) Gemini client call (HTTP) – model:"gemini-2.5-flash"
- Main: Send prompt with context and receive candidates
- Inputs: API key, model, request text & context, optional timeout
- Returns: candidates text (string list)
- Edge cases: empty candidates; HTTP 4xx/5xx; timeout; invalid API key/model
- Dependencies: External Gemini API; network; quota/rate-limit (429)

5) Web fallback (Wikipedia/Google)
- Main: Retrieve public info when DB context is insufficient
- Inputs: question; language (vi/en); search parameters
- Returns: snippets/text for augmentation
- Edge cases: network failure; empty results; throttling; language mismatch
- Dependencies: Wikipedia API and/or Google Programmable Search

Assumptions & Invariants
- No seeding; tests must be idempotent and avoid assuming specific records
- Language defaults to Vietnamese when null/invalid
- maxContext clamped to [1..~32]

Risk Register
- Latency: LLM + network (target P95 < 15s)
- Rate limiting / transient errors: 429/5xx → require backoff in tests
- Empty candidates from LLM → graceful fallback
- I18N consistency VN/EN (lexical checks, not exact phrasing)
- Concurrency: 3–5 concurrent calls within 30–60s soft budget

Categories (mapping to test plan)
- HappyPath, EdgeCase, ErrorHandling, I18N, Performance, Concurrency, ContextBehavior

Exit Criteria
- Function list analyzed with 1–5 points as per refined Phase 1 prompt
- Risks and categories aligned with `tests/AI_QA_TEST_PLAN.md`
