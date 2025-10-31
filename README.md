[![.NET](https://img.shields.io/badge/backend-.NET%208.0-blue)](https://dotnet.microsoft.com/) [![React](https://img.shields.io/badge/frontend-React%2018-blue)](https://react.dev/) [![Issues](https://img.shields.io/github/issues/binz0209/ViHis)](https://github.com/binz0209/ViHis/issues) [![Stars](https://img.shields.io/github/stars/binz0209/ViHis)](https://github.com/binz0209/ViHis) [![Forks](https://img.shields.io/github/forks/binz0209/ViHis)](https://github.com/binz0209/ViHis/network/members)
# ViHis – AI-assisted Vietnamese History

Nền tảng hỗ trợ học Lịch sử Việt Nam với AI (Gemini), bài trắc nghiệm và quản lý người dùng. Repo đã chuẩn hoá kiến trúc nhiều lớp và test theo 6 phase.

## 🚀 Tính năng
- **AI_QA**: Hỏi đáp lịch sử (Gemini + Mongo retriever) với fallback web khi thiếu ngữ cảnh
- **AUTH_JWT**: Đăng ký/đăng nhập JWT, `GET /me`, đổi mật khẩu
- **GEN_QUIZ**: Tạo quiz (MCQ/Essay), nộp bài, tính điểm, danh sách quiz của tôi

## 🛠️ Công nghệ
- Backend: ASP.NET 8 Web API (Controllers), MongoDB (MongoDB.Driver), JWT (HMAC)
- Frontend: React 18 + Vite + TypeScript + Axios
- AI: Gemini (hạ tầng tại `VietHistory.Infrastructure.Services`)
- Tests: xUnit + FluentAssertions (Integration Real cho AUTH/QUIZ; AI_QA có xử lý 429)

## 🧱 Kiến trúc
- Runtime: FE → Api(Controllers) → Application(Services/DTO) → Domain → Infrastructure(Mongo/JWT/AI) → MongoDB | Gemini → Api → FE
- Dependency: Api → Application → Domain ← Infrastructure (implements interfaces)
- Không trả Domain Entity trực tiếp; dùng DTO thuần cho I/O

## 📁 Cấu trúc thư mục
```
ViHis/
├── BackEnd/
│   ├── VietHistory.Api/                # Controllers, Program.cs
│   ├── VietHistory.Application/        # Service interfaces, DTOs
│   ├── VietHistory.Domain/             # Entities/Common
│   ├── VietHistory.Infrastructure/     # Mongo, Jwt, Gemini, Services
│   └── VietHistory.AI.Tests/           # xUnit tests (TCxx + Traits)
├── FrontEnd/                           # React + Vite + TS
├── prompts/
│   ├── MASTER_ARCH_PROMPT_VIHIS.md     # Prompt kiến trúc chuẩn ViHis
│   └── AI_PROMPT_PLAYBOOK.md           # Playbook 6 phase + templates
└── README.md
```

## 🔌 Endpoints
- Auth: POST `/api/v1/auth/register`, `/login`; GET `/me`; POST `/change-password`
- Quiz: POST `/api/v1/quiz/create`; GET `/{quizId}`; POST `/submit`; GET `/my-quizzes`; GET `/{quizId}/my-attempt`
- AI_QA: POST `/api/v1/ai/ask`
Chi tiết hợp đồng API và data flow: xem `prompts/MASTER_ARCH_PROMPT_VIHIS.md`.

## ⚙️ Thiết lập & chạy
### Backend
```bash
cd BackEnd/VietHistory.Api
# Chạy API (đảm bảo ENV đã thiết lập)
dotnet run
```
ENV cần có (ví dụ):
- `MONGO__CONNECTION_STRING`, `MONGO__DATABASE`
- `JWT__KEY`, `JWT__ISSUER`, `JWT__AUDIENCE`, `JWT__EXP_MIN`
- `GEMINI__API_KEY`, `GEMINI__MODEL` (nếu bật AI_QA real)

### Frontend
```bash
cd FrontEnd
npm install
npm run dev
```
`.env.example`: `VITE_API_BASE_URL=https://localhost:5001`

## 🧪 Testing & Coverage
- Traits: `Feature=AUTH_JWT|GEN_QUIZ|AI_QA`, `Category`, `Priority`, `Integration`
- Định danh TC: `TCxx_Given_When_Then`

Chạy theo feature:
```bash
cd BackEnd
# AUTH + QUIZ
dotnet test --filter "Feature=AUTH_JWT|Feature=GEN_QUIZ" -v minimal
# AI_QA (có thể gặp 429 real API)
dotnet test --filter "Feature=AI_QA" -v minimal || true
```
Xuất coverage HTML:
```bash
cd BackEnd
 dotnet test --collect:"XPlat Code Coverage" -v minimal || true
 export PATH="$PATH:$HOME/.dotnet/tools"
 reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport-final" "-reporttypes:Html;HtmlSummary"
 open coveragereport-final/index.html
```
Tình trạng: AUTH_JWT & GEN_QUIZ pass; AI_QA Integration có thể 429 (được ghi nhận).

## 🧭 Quy ước phát triển
- Controllers mỏng; business ở Application; IO ở Infrastructure
- Tên biến/hàm rõ nghĩa; guard clauses; không hardcode secrets
- Khi đổi hành vi, bổ sung/điều chỉnh test; đảm bảo `dotnet test` pass

## 📚 Tài liệu
- Kiến trúc & API contracts: `prompts/MASTER_ARCH_PROMPT_VIHIS.md`
- Playbook/6 phase: `prompts/AI_PROMPT_PLAYBOOK.md`
- Kết quả theo phase: `prompts/results/*`

---

Last updated: 2025-10-31

