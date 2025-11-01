# Master Architectural Prompt – ViHis

Dùng prompt này để định hướng thiết kế/kiểm thử/triển khai cho ViHis (.NET 8 + MongoDB + React/Vite), thay cho các mẫu không phù hợp (EF/SQLite/Minimal API).

---

## PROJECT
- ViHis – AI-assisted Vietnamese History (Backend ASP.NET 8 Web API + MongoDB + JWT; Frontend React 18 + Vite + TS; AI Gemini).

## TECH STACK
- Backend: ASP.NET 8 Web API (Controllers), MongoDB (MongoDB.Driver), JWT (HMAC), xUnit + FluentAssertions
- Projects: `VietHistory.Api`, `VietHistory.Application`, `VietHistory.Domain`, `VietHistory.Infrastructure`, `VietHistory.AI.Tests`
- Frontend: React 18 + TypeScript + Vite + Axios
- AI: Gemini via Infrastructure service

## ARCHITECTURE (Chuẩn ViHis)
- Runtime flow: FE → Api(Controllers) → Application(Services/DTO) → Domain(Entities/Rules) → Infrastructure(Mongo/JWT/AI) → MongoDB | Gemini → quay về Api → FE
- Dependency direction: Api → Application → Domain ← Infrastructure(implements interfaces)
- Không trả Domain Entity ra ngoài; dùng DTO (request/response)

---

## DATA MODELS (cốt lõi – rút gọn)
- User { id, username, email, passwordHash, createdAt, updatedAt }
- Quiz { id, creatorId, topic, multipleChoiceCount, essayCount, questions[] }
- EmbeddedQuizQuestion { id, type: 'multipleChoice'|'essay', question, options[], correctAnswerIndex? }
- QuizAttempt { id, quizId, userId, answers: Record<questionId,string>, score, totalQuestions, completedAt }
- AI_QA: AiAskRequest { question, language, maxContext }, AiAnswer { answer, model, sources? }

---

## REQUIREMENTS (toàn dự án)
1) Kiến trúc nhiều lớp sạch: Api → Application → Domain ← Infrastructure; SoC & SOLID
2) RESTful APIs (v1):
   - Auth: POST /api/v1/auth/register, /login; GET /me; POST /change-password
   - Quiz: POST /api/v1/quiz/create; GET /{quizId}; POST /submit; GET /my-quizzes; GET /{quizId}/my-attempt
   - AI_QA: POST /api/v1/ai/ask
3) Validation & lỗi: 400/401/403/404/409/500; không rò rỉ chi tiết hạ tầng; dùng ProblemDetails khi phù hợp
4) Cấu hình qua ENV/appsettings:
   - Mongo: MONGO__CONNECTION_STRING, MONGO__DATABASE
   - JWT: JWT__KEY, JWT__ISSUER, JWT__AUDIENCE, JWT__EXP_MIN
   - Gemini: GEMINI__API_KEY, GEMINI__MODEL, GOOGLE_SEARCH_KEY/CX (tuỳ chọn)
5) Testing: xUnit + FluentAssertions; Integration Real cho AUTH/QUIZ; AI_QA chấp nhận 429 (backoff)
6) Coverage mục tiêu ≥85% (100% lớp trọng yếu Application/Services)

---

## DESIGN APPROACH
- Controllers mỏng; business ở Application Services; IO ở Infrastructure
- Type-safe: C# DTOs; TS types ở FE
- Async/await toàn bộ IO; guard clauses; đặt tên rõ ràng
- Không hardcode secrets; đọc từ ENV/launchSettings

---

## BACKEND – Structure (thư mục/dự án)
- `VietHistory.Api/`
  - `Controllers/` (AuthController, QuizController, AiController, …)
  - `Program.cs` (DI, middleware, AuthN/Z, swagger – không chứa business)
  - `Properties/launchSettings.json`
- `VietHistory.Application/`
  - `Services/` (interfaces; orchestration nghiệp vụ)
  - `DTOs/` (request/response)
- `VietHistory.Domain/`
  - `Entities/`, `Common/`
- `VietHistory.Infrastructure/`
  - `Mongo/` (MongoSettings, IMongoContext, MongoContext)
  - `Services/` (JwtService, QuizService, GeminiStudyService, …)
- `VietHistory.AI.Tests/` (Unit/Integration – TCxx, Traits)

---

## API CONTRACTS (rút gọn)
### Auth
- POST `/api/v1/auth/register` → 200 { token, user:{ id, username, email } } | 400
- POST `/api/v1/auth/login` → 200 { token, user } | 401
- GET `/api/v1/auth/me` (Bearer) → 200 { id, username, email } | 401 | 404
- POST `/api/v1/auth/change-password` (Bearer) → 200 { message } | 400 | 401

### Quiz
- POST `/api/v1/quiz/create` → 200 QuizDto
- GET `/api/v1/quiz/{quizId}` → 200 QuizDto | 404
- POST `/api/v1/quiz/submit` → 200 QuizAttemptDto | 404
- GET `/api/v1/quiz/my-quizzes` → 200 QuizDto[]
- GET `/api/v1/quiz/{quizId}/my-attempt` (Bearer) → 200 QuizAttemptDto | 404 | 401

### AI_QA
- POST `/api/v1/ai/ask` → 200 { answer, model, sources? } | 429 | 500

---

## DATA FLOW (ví dụ)
- Submit Quiz: Controller → IQuizService.SubmitQuizAsync(userId, req) → Mongo load Quiz → chấm điểm MCQ → lưu QuizAttempt → trả AttemptDto
- Auth Login: Controller → Mongo Users + PasswordHasher → JwtService.GenerateToken → trả AuthResponse
- AI_QA Ask: Controller → GeminiStudyService (HTTP Gemini + retriever Mongo) → tổng hợp → trả AiAnswer

---

## VALIDATION & ERROR POLICY
- Guard tại Application Services; map exception → HTTP chính xác
- ProblemDetails cho 4xx/5xx; thông điệp ngắn gọn, không lộ secrets

---

## CONFIG/ENV
- `appsettings.json` + ENV overrides (Docker/CI friendly)
- Biến môi trường như phần REQUIREMENTS; tài liệu hoá `.env`/UserSecrets khi dev

---

## TESTING GUIDELINES
- Traits: Feature=AUTH_JWT|GEN_QUIZ|AI_QA; Category=HappyPath|EdgeCase|ErrorHandling|Performance|I18N|Concurrency; Priority=P0|P1|P2
- Tên test: `TCxx_Given_When_Then`
- Integration Real cho AUTH/QUIZ; AI_QA chấp nhận 429 (assert hợp lý)
- Kết xuất coverage HTML bằng `reportgenerator`

---

## FRONTEND – Structure (React/Vite/TS)
- `src/`
  - `components/` (presentational, ≤200 lines)
  - `pages/` (containers; dùng hooks/services)
  - `services/` (axios instance + modules; baseURL từ `import.meta.env.VITE_API_BASE_URL`)
  - `types/` (User, Quiz, Attempt, Auth…)
  - `hooks/` (useAuth, useQuiz…)
  - `context/` (AuthContext nếu cần)
  - `utils/`
- `.env.example`: `VITE_API_BASE_URL=https://localhost:5001`
- Quy tắc: không gọi API trực tiếp trong component; validate trước submit; loading/error states; retry GET có backoff (≤3 lần) cho endpoints phù hợp

---

## LỆNH NHANH
```
cd BackEnd
dotnet test --filter "Feature=AUTH_JWT|GEN_QUIZ|AI_QA" -v minimal
dotnet test --collect:"XPlat Code Coverage" -v minimal || true
export PATH="$PATH:$HOME/.dotnet/tools"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport-final" "-reporttypes:Html;HtmlSummary"
open coveragereport-final/index.html
```

---

## CHECKLIST
- ≥ 40 TC/feature; TCxx + Traits chuẩn; GWT rõ ràng
- Coverage đạt mục tiêu; coverage HTML tạo được
- Không rò rỉ secrets; build/lint sạch; test time không tăng >10%
- Docs: flow, lệnh chạy, coverage link, ENV
