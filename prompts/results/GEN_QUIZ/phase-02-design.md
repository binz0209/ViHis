# GEN_QUIZ – Phase 2: Design (Result)

Date: 2025-10-30

Artifacts
- Test Matrix: `tests/QUIZ_TEST_PLAN.md` (≥40 GWT cases) covering Create/Get/Submit/My endpoints, Security, Scoring, Performance.
- Traits Standard:
  - `[Trait("Feature", "GEN_QUIZ")]`
  - `[Trait("Category", "HappyPath|EdgeCase|ErrorHandling|Security|Performance|Scoring")]`
  - `[Trait("Priority", "P0|P1|P2")]`
  - `[Trait("Integration", "Real|Mocked")]` (Mocked for FE contract while backend endpoint chưa sẵn sàng)

Global Phase 2 Prompt (GWT) Used — NEW (standardized)
```
Generate comprehensive unit/integration test cases using Given–When–Then for the following functions:

[PASTE FUNCTION LIST FROM PHASE 1 HERE]

For each function, include:
- Happy path scenarios
- Edge cases (boundary values)
- Error scenarios
- Integration/state interaction (where applicable)

Constraints:
- Real API only | No seed | No mock (unless feature says otherwise)
- Add Traits: Feature, Category, Priority, Integration
- Prefer pattern-based assertions for idempotence
```

Applied Input (from Phase 1)
- Endpoints: create, get, submit, my-quizzes, my-attempt (+ scoring logic)
- Contracts: `FrontEnd/src/services/api.ts` types for FE
- Context rules just added from QuizController:
  - Guest support via `X-Machine-Id` → userId = `guest_{machineId}` if unauthenticated
  - `[Authorize]` required for `GET /api/v1/quiz/{quizId}/my-attempt`

Full Matrix (copied for convenience)

Create (P0/P1)
1. GIVEN valid topic + counts; WHEN create; THEN returns QuizDto with questions.
2. GIVEN zero MCQ + some essay; WHEN create; THEN valid QuizDto.
3. GIVEN negative count; WHEN create; THEN 400.
4. GIVEN very large count; WHEN create; THEN 400 or limited.
5. GIVEN empty topic; WHEN create; THEN 400.
6. GIVEN unicode topic; WHEN create; THEN success.
7. GIVEN duplicate create; WHEN called twice; THEN two distinct quizIds.
8. GIVEN unexpected fields; WHEN create; THEN ignored.
9. GIVEN auth required (if any); WHEN missing token; THEN 401/403.
10. GIVEN slow generation; WHEN create; THEN within timeout threshold.

Get Quiz (P0/P1)
11. GIVEN quizId exists; WHEN get; THEN full QuizDto structure.
12. GIVEN unknown quizId; WHEN get; THEN 404.
13. GIVEN malformed id; WHEN get; THEN 400.
14. GIVEN unauthorized (if private); WHEN get; THEN 403.

Submit (Scoring) (P0)
15. GIVEN correct MCQ answers; WHEN submit; THEN score equals total MCQ correct.
16. GIVEN all wrong MCQ; WHEN submit; THEN score 0.
17. GIVEN partial MCQ; WHEN submit; THEN partial scoring.
18. GIVEN essay answers; WHEN submit; THEN returns attempt with placeholders (if manual grading) or heuristic.
19. GIVEN missing answers map; WHEN submit; THEN 400.
20. GIVEN extra answers for unknown question; WHEN submit; THEN ignore/400 per design.
21. GIVEN invalid option index; WHEN submit; THEN 400.
22. GIVEN repeated submit same quiz; WHEN submit again; THEN last attempt returned or new attempt policy documented.

My Quizzes / My Attempt (P1)
23. GIVEN created quizzes; WHEN my-quizzes; THEN returns list including latest.
24. GIVEN no quizzes; WHEN my-quizzes; THEN empty list.
25. GIVEN attempt exists; WHEN my-attempt; THEN returns attempt.
26. GIVEN no attempt; WHEN my-attempt; THEN 404.
27. GIVEN another user's quiz; WHEN my-attempt; THEN 404/403.

Edge Cases & Validation (P1/P2)
28. GIVEN topic with long length; WHEN create; THEN handled.
29. GIVEN counts sum 0; WHEN create; THEN 400.
30. GIVEN non-integer counts; WHEN create; THEN 400.
31. GIVEN answer map with nulls; WHEN submit; THEN 400.
32. GIVEN duplicate question IDs in answers; WHEN submit; THEN handled.
33. GIVEN whitespace-only essay; WHEN submit; THEN handled gracefully.

Performance/Concurrency (P2)
34. GIVEN 5 parallel creates; WHEN run; THEN all succeed within 60s.
35. GIVEN 10 parallel submits; WHEN run; THEN success rate 100%, time budget respected.
36. GIVEN large quiz (if allowed); WHEN get; THEN still < 5s.

Security (P1)
37. GIVEN token missing; WHEN my endpoints; THEN 401.
38. GIVEN tampered attempt id; WHEN my-attempt; THEN 404/403.
39. GIVEN XSS-like input in essay; WHEN create/submit; THEN safely stored/returned (no script execution on FE tests).

Data Integrity (P2)
40. GIVEN created quiz; WHEN get; THEN question order stable.
41. GIVEN submit; WHEN re-fetch quiz; THEN quiz content unchanged.
42. GIVEN multi-submit; WHEN compare attempts; THEN consistent scoring.

Exit Criteria
- ≥40 test cases designed in GWT using the new prompt; ready for Phase 3 implementation (FE contract tests now; backend integration tests khi controller sẵn sàng).
