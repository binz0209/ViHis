GEN_QUIZ – Phase 3: Code (Result)

Date: 2025-10-31

Scope
- Implement minimal QuizService to unblock QuizController endpoints; scaffold QuizGenerationService for DI.
- Add Unit + Integration (Real Mongo, no JWT, guest via X-Machine-Id).

Key Edits
- Added BackEnd/VietHistory.Infrastructure/Services/QuizService.cs (implements IQuizService).
- Added BackEnd/VietHistory.Infrastructure/Services/QuizGenerationService.cs (stub for DI).
- Tests added:
  - BackEnd/VietHistory.AI.Tests/GEN_QUIZ_UnitTests.cs (validation, generation, scoring, my-*).
  - BackEnd/VietHistory.AI.Tests/GEN_QUIZ_IntegrationTests.cs (controller endpoints, guest flow).

Current Test Count
- Total implemented for GEN_QUIZ: 22 (Unit + Integration=Real), all passing.
- Target per plan: 42 → remaining 20 to be added next.

Notes
- Real Mongo: vihis_test (Atlas). No seed; assertions by structure/behavior.
- No JWT in tests; my-attempt tested only for unauthorized behavior.

