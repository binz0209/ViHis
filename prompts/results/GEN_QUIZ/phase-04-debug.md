GEN_QUIZ – Phase 4: Debug (Result)

Date: 2025-10-31

Failures observed (during expansion to 22 tests)
- NullReference in integration test when unwrapping ActionResult → fixed by reading OkObjectResult.Value or .Value fallback.
- FormatException for non-ObjectId quizId in GetQuiz → fixed test by using a generated ObjectId string for unknown id.

Stability notes
- Real Mongo used; no seed. Tests are idempotent (new documents each run).
- Controller guest flow relies on X-Machine-Id; added header in test setup.

Next
- Add the remaining cases (to 42) including boundary/large counts, perf/concurrency light checks, and data integrity scenarios.

