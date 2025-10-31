# ViHis – AI-Assisted Testing Playbook (Chuẩn Senior)

Tài liệu này là prompt mẫu HOÀN CHỈNH, áp dụng trực tiếp cho dự án ViHis (.NET 8 + MongoDB + React). Dùng cho cả phân tích, thiết kế test, sinh test code, chạy & debug, tối ưu coverage, và tài liệu hóa theo 6 phase.

---

## Architecture (chuẩn ViHis)
- Runtime flow: FrontEnd (React) → HTTP → VietHistory.Api (Controllers) → VietHistory.Application (Services/DTO) → VietHistory.Domain (Entities/Rules) → VietHistory.Infrastructure (Mongo/JWT/AI) → MongoDB | Gemini → quay về API → FE.
- Dependency direction: Api → Application → Domain ← Infrastructure (implements interfaces).  Không trả Domain Entity trực tiếp ra ngoài; dùng DTO.

---

## Phase 1 – Testing Specification (Analysis & Selection)
Context:
- Dự án ViHis (.NET 8, xUnit, FluentAssertions). Kiến trúc nhiều lớp như trên. Các feature trọng tâm: AI_QA (Gemini), AUTH_JWT, GEN_QUIZ.

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

## Phase 3 – Test Code Generation (xUnit + FluentAssertions)
Role: Expert .NET test engineer.

Requirements:
- .NET 8, xUnit, FluentAssertions. Không thêm framework ngoài nếu không cần.
- Ưu tiên Integration Real cho AUTH_JWT, GEN_QUIZ (Mongo/JWT thật). AI_QA có thể bị rate-limit 429: cho phép backoff/accept retry-friendly assertions.
- Mỗi TC là một `[Fact]` độc lập, tên hàm theo `TCxx_Given_When_Then`.
- Không ghi secrets mới; đọc cấu hình từ ENV/launchSettings khi cần.

Output:
- Mã C# test hoàn chỉnh (biên dịch được), đặt vào `BackEnd/VietHistory.AI.Tests/*.cs` tương ứng.

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
- Endpoints: `POST /api/v1/auth/register`, `POST /api/v1/auth/login`, `GET /api/v1/auth/me`, `POST /api/v1/auth/change-password`.
- Real Mongo + `JwtService`. Test 401/403/404, claim/expiry/issuer/audience, đổi mật khẩu.
- Chú ý hành vi hiện tại: thiếu token ở change-password đang 404 (ghi nhận nếu chưa đổi policy).

### B) GEN_QUIZ
- Endpoints: create/get/submit/my-quizzes/my-attempt.
- Service: `QuizService` (Mongo `Quizzes`, `QuizAttempts`), có `QuizGenerationService` placeholder.
- Scoring: MCQ tự động; essay không chấm điểm; đảm bảo ổn định thứ tự câu hỏi và multi-submit.

### C) AI_QA (Gemini)
- Service: `GeminiStudyService` (HTTP → Gemini + retriever Mongo). Real API có thể 429 → chấp nhận retry/backoff, hoặc đánh dấu “current behavior”.
- Assertions: từ khóa lịch sử, chiều dài tối thiểu, thời gian < ngưỡng mềm.

---

## Lệnh nhanh
```
cd BackEnd
dotnet test --filter "Feature=AUTH_JWT|GEN_QUIZ|AI_QA" -v minimal
dotnet test --collect:"XPlat Code Coverage" -v minimal || true
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


