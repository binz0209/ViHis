# AI_QA – Phase 2: Design (Result)

Date: 2025-10-30
Artifacts Produced
- Test Matrix: `tests/AI_QA_TEST_PLAN.md` with 42 Given–When–Then cases across categories:
  - HappyPath (1–10), EdgeCase (11–20), ErrorHandling (21–28), I18N (29–32), Performance (33–36), ContextBehavior (37–42)
- Traits Standard (to apply in all tests):
  - `[Trait("Feature", "AI_QA")]`
  - `[Trait("Category", "HappyPath|EdgeCase|ErrorHandling|Performance|I18N|Concurrency")]`
  - `[Trait("Priority", "P0|P1|P2")]`
  - `[Trait("Integration", "Real")]`

Global Prompt Applied (GWT Best Practice)
- Input: Function list from Phase 1 (AskAsync, AiController.Ask, Mongo query layer, Gemini client call, Web fallback)
- Output: GWT cases below (sample highlights; full list in `tests/AI_QA_TEST_PLAN.md`).

GWT Cases (selected, per function)
1) GeminiStudyService.AskAsync
- Given Mongo has relevant chunks; When ask VN event; Then answer contains event keywords and model name. [HappyPath,P0]
- Given empty question; When ask; Then returns graceful generic answer. [EdgeCase,P1]
- Given invalid API key; When ask; Then handled without unhandled exception; non-empty error/fallback message. [ErrorHandling,P0]
- Given language=null; When ask; Then defaults to Vietnamese lexical set. [I18N,P1]
- Given 3 concurrent asks; When execute; Then all succeed within 30s. [Performance/Concurrency,P1]
- Given maxContext=0 and 1000; When ask; Then clamped within valid bounds; answers non-empty. [ContextBehavior,P2]

2) AiController.Ask (POST /api/v1/ai/ask)
- Given valid JSON body; When post; Then 200 with AiAnswer shape (answer, model). [HappyPath,P0]
- Given malformed JSON; When post; Then 400. [ErrorHandling,P1]
- Given oversized payload; When post; Then handled (400 or 413) without server crash. [ErrorHandling,P1]

3) Mongo query layer (RAG)
- Given no matching records; When ask; Then web-driven answer length > 30. [EdgeCase,P1]
- Given large dataset; When ask; Then completes under 15s P95. [Performance,P1]

4) Gemini client call
- Given transient 429; When backoff; Then eventually succeed or coherent message; no unhandled exception. [ErrorHandling,P0]
- Given empty candidates; When ask; Then fallback explanatory text. [ErrorHandling,P1]

5) Web fallback
- Given Wikipedia unreachable; When ask; Then use Google/LLM only and answer remains > 30 chars. [ErrorHandling,P1]
- Given English question; When fallback; Then answer keywords in English. [I18N,P1]

Assertions & Idempotence
- Prefer structural patterns: min length, year regex `\d{3,4}`, VN/EN lexical sets
- Avoid asserting specific DB records; keep Real API only; no seed; introduce retry/backoff only in tests marked Performance/Integration

Full Matrix (copied for convenience)

Happy Path (P0)
1. GIVEN Mongo has relevant chunks; WHEN ask VN event; THEN answer contains event keywords and model name.
2. GIVEN Mongo has person info; WHEN ask biography; THEN answer includes key terms.
3. GIVEN mixed DB+web; WHEN ask general topic; THEN prefers DB context.
4. GIVEN English question; WHEN ask EN; THEN English answer keywords.
5. GIVEN reasonable context=5; WHEN ask; THEN non-empty answer under 15s.
6. GIVEN short query; WHEN ask; THEN concise answer (>30 chars).
7. GIVEN long but valid query; WHEN ask; THEN returns without truncation errors.
8. GIVEN history timeline query; WHEN ask; THEN chronological phrasing present.
9. GIVEN cause-effect query; WHEN ask; THEN contains "nguyên nhân" or equivalent.
10. GIVEN compare dynasties query; WHEN ask; THEN mentions ≥2 dynasties.

Edge Cases (P1)
11. GIVEN empty question; WHEN ask; THEN graceful generic response.
12. GIVEN whitespace-only question; WHEN ask; THEN generic response.
13. GIVEN special symbols; WHEN ask; THEN no crash and meaningful text.
14. GIVEN unicode accents; WHEN ask; THEN preserved text handling.
15. GIVEN language=null; WHEN ask; THEN defaults to VN.
16. GIVEN invalid language code; WHEN ask; THEN fallback to VN.
17. GIVEN context=0; WHEN ask; THEN clamp to min context.
18. GIVEN context>max (1000); WHEN ask; THEN clamp to internal max.
19. GIVEN very long question (~1000 chars); WHEN ask; THEN success.
20. GIVEN repeated words/noise; WHEN ask; THEN denoised sensible answer.

Error Handling (P0/P1)
21. GIVEN invalid API key; WHEN ask; THEN handled (no unhandled exception) and non-empty message.
22. GIVEN invalid model; WHEN ask; THEN handled gracefully.
23. GIVEN transient 429; WHEN multiple asks with backoff; THEN eventually succeed or safe message.
24. GIVEN HTTP timeout; WHEN ask; THEN returns fallback message.
25. GIVEN Mongo intermittent issue; WHEN ask; THEN web fallback text returned.
26. GIVEN empty candidates from LLM; WHEN ask; THEN fallback explanatory text.
27. GIVEN network DNS hiccup; WHEN ask; THEN retry then error message coherent.
28. GIVEN Wikipedia unavailable; WHEN ask; THEN use Google or LLM only.

I18N (P1)
29. GIVEN Vietnamese question; WHEN ask; THEN Vietnamese lexical set present.
30. GIVEN English question; WHEN ask; THEN English lexical set present.
31. GIVEN mixed-language; WHEN ask; THEN primary language follows request.
32. GIVEN French label; WHEN ask; THEN still returns reasonable answer (fallback EN/VN allowed).

Performance (P1)
33. GIVEN medium question; WHEN ask; THEN P95 < 15s.
34. GIVEN 3 concurrent asks; WHEN run; THEN all < 30s and valid.
35. GIVEN 5 parallel asks; WHEN run; THEN all succeed within 60s.
36. GIVEN repeated asks same question; WHEN run; THEN average time not increasing > 20%.

Context Behavior (P2)
37. GIVEN maxContext=1; WHEN ask; THEN answer remains coherent.
38. GIVEN maxContext=10; WHEN ask; THEN richer details (length threshold higher than case 37).
39. GIVEN no relevant Mongo data; WHEN ask; THEN web-driven answer length > 30.
40. GIVEN DB has single short chunk; WHEN ask; THEN answer references chunk topic.
41. GIVEN DB has many chunks; WHEN ask; THEN answer not obviously contradictory.
42. GIVEN question about dates; WHEN ask; THEN answer contains a year-like pattern (\d{3,4}).

Exit Criteria (Phase 2)
- ≥ 40 cases designed with GWT style and full Traits
- Ready to implement (Phase 3)
