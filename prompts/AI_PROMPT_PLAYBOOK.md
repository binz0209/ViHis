# ViHis – AI-Assisted Testing Playbook (Chuẩn Senior)

Tài liệu này là prompt mẫu HOÀN CHỈNH, áp dụng trực tiếp cho dự án ViHis (.NET 8 + MongoDB + React). Dùng cho cả phân tích, thiết kế test, sinh test code, chạy & debug, tối ưu coverage, và tài liệu hóa theo 6 phase.

---

## Architecture
- Runtime flow: FrontEnd (React) → HTTP → VietHistory.Api (Controllers) → VietHistory.Application (Services/DTO) → VietHistory.Domain (Entities/Rules) → VietHistory.Infrastructure (Mongo/JWT/AI) → MongoDB | Gemini → quay về API → FE.
- Dependency direction: Api → Application → Domain ← Infrastructure (implements interfaces).  Không trả Domain Entity trực tiếp ra ngoài; dùng DTO.

---

## Phase 1 – Testing Specification (Analysis & Selection)
Context:
- Dự án ViHis (.NET 8, xUnit, FluentAssertions). Kiến trúc nhiều lớp như trên. Các feature trọng tâm: AUTH_JWT, GEN_QUIZ, AI_QA (Gemini), CHAT, TEXT_INGEST.

Role:
- Expert software test engineer & prompt engineer.

Goal:
- Xác định toàn bộ phương thức/endpoint có logic cần Unit/Integration Test để đạt coverage mục tiêu (≥ 85% toàn module; 100% cho lớp trọng yếu Application/Service).

Requirements:
- Bỏ qua getters/setters hoặc helpers thuần dữ liệu.
- Bao gồm Controller + Service nếu có validation, branching, gọi hạ tầng, hoặc xử lý exception.
- Liệt kê dependencies có thể mock (nếu được phép) và dependencies bắt buộc Real (Mongo/JWT/Gemini).
- Ghi rõ edge cases: null/invalid/missing records, unauthorized, 4xx/5xx, branch-specific.
- Đề xuất cả happy-path và failure-path.

Output Format (Markdown):
### Functions to Test (Ranked)
1. FunctionName(parameters)
   - Main purpose: <1 dòng>
   - Inputs: <tham số: kiểu + ý nghĩa>
   - Returns: <kiểu + ý nghĩa>
   - Dependencies: <services/repos/APIs>
   - Logic branches: <điều kiện chính cần cover>
   - Edge cases: <liệt kê>
   - Test type: <Unit | Integration (Real|Mocked)>
   - Suggested Test Names (TCxx): TC01_..., TC02_...

### Prioritization
- High / Medium / Low và lý do (tác động nghiệp vụ, độ phức tạp, rủi ro).

### Acceptance (Phase 1)
- Danh sách hàm/endpoint có đủ inputs/returns/deps/branches/edge cases + ranking.
- Mục tiêu coverage rõ ràng theo file/module; ghi chú ràng buộc Real API.

---

## Phase 2 – Test Case Design (Test Matrix, Given–When–Then)
Context:
- Sinh ma trận test cho lớp/endpoint được chọn (ví dụ: `AuthController`, `QuizController`, `GeminiStudyService`).

Instructions:
1) Với mỗi phương thức, liệt kê 4–8 test cases:
   - Happy Path
   - Edge Case
   - Error Handling
   - Integration (Real deps hoặc mocked deps – nêu rõ)
2) Sử dụng định dạng Given–When–Then ngắn gọn.
3) Ghi Traits chuẩn: Feature | Category | Priority | Integration.

Output Format (Markdown Table):
| Category | TC Name (TCxx_) | Given | When | Then | Traits |
|---|---|---|---|---|---|
| HappyPath | TC01_Login_Valid_Returns_Token | user tồn tại | POST /auth/login | 200 + token | Feature=AUTH_JWT; P0; Integration=Real |

Acceptance (Phase 2):
- Ma trận đủ phủ các nhánh chính/ngoại lệ; tên TCxx rõ ràng; Traits đầy đủ.

---

## Phase 3 – Test Code Generation (xUnit + FluentAssertions + AAA Pattern)
Role: Expert .NET test engineer.

Requirements:
- .NET 8, xUnit, FluentAssertions. Không thêm framework ngoài nếu không cần.
- **BẮT BUỘC**: Mỗi test method PHẢI dùng **Arrange-Act-Assert (AAA)** pattern với comments rõ ràng:
  ```csharp
  [Fact]
  public async Task TC01_MethodName_Given_When_Then()
  {
      // Arrange - Setup test data, mocks, dependencies
      var testData = ...;
      await _repository.InsertAsync(testData);
      
      // Act - Execute the code under test
      var result = await _controller.MethodUnderTest(...);
      
      // Assert - Verify the outcome
      result.Should().NotBeNull();
      result.StatusCode.Should().Be(200);
  }
  ```
- **Lưu ý**: Phase 2 (Design) dùng **Given-When-Then (GWT)** cho documentation, nhưng Phase 3 (Code) **BẮT BUỘC** dùng **AAA** cho implementation. Đây là .NET industry standard.
- Ưu tiên Integration Real cho AUTH_JWT, GEN_QUIZ (Mongo/JWT thật). AI_QA có thể bị rate-limit 429: cho phép backoff/accept retry-friendly assertions.
- Mỗi TC là một `[Fact]` độc lập, tên hàm theo `TCxx_Given_When_Then` (GWT format cho method name, nhưng code structure dùng AAA).
- Không ghi secrets mới; đọc cấu hình từ ENV/launchSettings khi cần.

Code Structure Requirements:
- **Arrange section**: Khởi tạo objects, mocks, test data, setup dependencies. Phải có comment `// Arrange` kèm mô tả ngắn.
- **Act section**: Gọi method/endpoint under test. Phải có comment `// Act`.
- **Assert section**: Verify kết quả bằng FluentAssertions. Phải có comment `// Assert`.
- Không trộn lẫn Arrange/Act/Assert trong cùng một section.

Output:
- Mã C# test hoàn chỉnh (biên dịch được), đặt vào `BackEnd/VietHistory.AI.Tests/*.cs` tương ứng.
- **Tất cả test methods phải có AAA structure rõ ràng**.

---

## Phase 4 – Run & Debug Tests
Instructions:
- Chạy: `cd BackEnd && dotnet test -v minimal`.
- Với coverage: `dotnet test --collect:"XPlat Code Coverage"` rồi tạo HTML bằng `reportgenerator`.
- Phân tích lỗi, sửa, và chạy lại đến khi pass.

Output:
- Tất cả test pass; build/lint sạch.

---

## Phase 5 – Optimize & Coverage
Mục tiêu:
- Nâng coverage ≥ 85% (hoặc 100% lớp chính). Thêm test edge/exception, perf/concurrency cơ bản (P95, parallel logins/submits).

Deliverables:
- Báo cáo coverage HTML (đường dẫn: `BackEnd/coveragereport-final/index.html`).

---

## Phase 6 – Documentation & Demo
Nội dung:
- Tổng hợp ma trận test, lệnh chạy nhanh, liên kết coverage, ghi chú hành vi hiện tại vs. mong muốn.
- Demo flows chủ đạo (AUTH đăng ký/đăng nhập/đổi mật khẩu; GEN_QUIZ tạo/nộp/my endpoints; AI_QA hỏi đáp thực).

---

## Feature Guides (áp dụng cho ViHis)

### A) AUTH_JWT
- **Endpoints**: `POST /api/v1/auth/register`, `POST /api/v1/auth/login`, `GET /api/v1/auth/me`, `POST /api/v1/auth/change-password`.
- **Architecture**: Controller → `JwtService` → MongoDB. Có Service Layer → Có Unit Tests.
- **Dependencies**: Real MongoDB + `JwtService` (logic phức tạp: generate token, validate token, hash password).
- **Test Focus**: 401/403/404, claim/expiry/issuer/audience, đổi mật khẩu, duplicate registration, invalid credentials.
- **Chú ý**: Thiếu token ở change-password đang 404 (ghi nhận nếu chưa đổi policy).

### B) GEN_QUIZ
- **Endpoints**: `POST /api/v1/quiz/create`, `GET /api/v1/quiz/{id}`, `POST /api/v1/quiz/submit`, `GET /api/v1/quiz/my-quizzes`, `GET /api/v1/quiz/my-attempts`.
- **Architecture**: Controller → `QuizService` → `QuizGenerationService` → MongoDB/Gemini API.
- **Dependencies**: Real MongoDB, Real Gemini API (quiz generation). Có Service Layer → Có Unit Tests.
- **Test Focus**: MCQ tự động scoring; essay không chấm điểm; ổn định thứ tự câu hỏi; multi-submit; invalid quiz IDs.
- **Chú ý**: `QuizGenerationService` gọi Gemini API thật để generate questions.

### C) AI_QA (Gemini)
- **Endpoints**: `POST /api/v1/ai/ask`.
- **Architecture**: Controller → `GeminiStudyService` → Gemini API + MongoDB RAG (KWideRetriever).
- **Dependencies**: Real Gemini API, Real MongoDB (RAG retrieval). **Có thể bị rate limit 429**.
- **Test Focus**: Vietnamese/English/French questions; RAG context; web fallback; từ khóa lịch sử; chiều dài tối thiểu; P95 < 15s.
- **Chú ý**: Real API có thể 429 → chấp nhận retry/backoff, hoặc đánh dấu "current behavior" cho failed tests.

### D) CHAT
- **Endpoints**: `GET /api/v1/chat/boxes`, `POST /api/v1/chat/history`, `GET /api/v1/chat/history/{boxId}`, `PUT /api/v1/chat/history/{boxId}/name`, `DELETE /api/v1/chat/history/{boxId}`.
- **Architecture**: Controller → MongoDB (trực tiếp, **KHÔNG có Service Layer**). Chỉ có Integration Tests.
- **Dependencies**: Real MongoDB only. Logic đơn giản (CRUD operations) → Không cần Unit Tests.
- **Test Focus**: GetChatBoxes với userId/machineId; SaveHistory (create new/update existing); cascade delete messages; GetHistory với sorting; RenameBox; DeleteBox idempotent.
- **Chú ý**: `GetHistory` không found → 200 OK với empty messages (không phải 404). `DeleteBox` idempotent → 200 OK kể cả khi boxId không tồn tại.

### E) TEXT_INGEST
- **Endpoints**: `POST /api/v1/ingest/preview`, `POST /api/v1/ingest/pdf`, `GET /api/v1/ingest/chunks`, `GET /api/v1/ingest/sources`, `GET /api/v1/ingest/source/{id}`.
- **Architecture**: Controller → `FallbackAIngestor` → TextNormalizer/HeaderFooterDetector/SentenceTokenizer/ChunkPack/PdfTextExtractor → MongoDB + Gemini Embedding API.
- **Dependencies**: Real MongoDB, Real Gemini Embedding API (cho embeddings), PdfPig library (PDF parsing). Có nhiều Service Layers → Có Unit Tests (35 tests).
- **Test Focus**: PDF text extraction; normalization (CRLF, hyphen breaks, spaced letters); header/footer detection; sentence tokenization; chunk packing với overlap; embedding generation; pagination; invalid ObjectId format.
- **Chú ý**: Gemini Embedding API có thể rate limit; preview endpoint không lưu DB (chỉ preview 10 chunks đầu); IngestAndSave lưu Source + Chunks với embeddings vào MongoDB.

---

## Lệnh nhanh

### Chạy test theo feature
```bash
cd BackEnd
dotnet test --filter "Feature=AUTH_JWT" -v minimal
dotnet test --filter "Feature=GEN_QUIZ" -v minimal
dotnet test --filter "Feature=AI_QA" -v minimal
dotnet test --filter "Feature=CHAT" -v minimal
dotnet test --filter "Feature=TEXT_INGEST" -v minimal
```

### Chạy tất cả tests
```bash
cd BackEnd
dotnet test --filter "Feature=AUTH_JWT|GEN_QUIZ|AI_QA|CHAT|TEXT_INGEST" -v minimal
```

### Chạy test với coverage và generate HTML report
```bash
cd BackEnd
# Bước 1: Chạy test với coverage
dotnet test --collect:"XPlat Code Coverage" -v minimal || true

# Bước 2: Generate HTML report
export PATH="$PATH:$HOME/.dotnet/tools"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport-final" "-reporttypes:Html;HtmlSummary"
open coveragereport-final/index.html
```

---

## Checklist tổng kết
- ≥ 40 test/feature; TCxx + Traits chuẩn; Given–When–Then rõ ràng
- Coverage đạt mục tiêu; báo cáo HTML tạo được
- Không rò rỉ secrets; build/lint sạch; không tăng thời gian test > 10%
- Tài liệu ngắn gọn: flow, lệnh chạy, coverage link


