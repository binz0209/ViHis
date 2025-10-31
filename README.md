[![.NET](https://img.shields.io/badge/backend-.NET%208.0-blue)](https://dotnet.microsoft.com/) [![React](https://img.shields.io/badge/frontend-React%2018-blue)](https://react.dev/) [![Tests](https://img.shields.io/badge/tests-165%20test%20cases-success)](https://github.com/binz0209/ViHis) [![Coverage](https://img.shields.io/badge/coverage-20.9%25-yellow)](https://github.com/binz0209/ViHis) [![Issues](https://img.shields.io/github/issues/binz0209/ViHis)](https://github.com/binz0209/ViHis/issues) [![Stars](https://img.shields.io/github/stars/binz0209/ViHis)](https://github.com/binz0209/ViHis) [![Forks](https://img.shields.io/github/forks/binz0209/ViHis)](https://github.com/binz0209/ViHis/network/members)

# ViHis – AI-assisted Vietnamese History Learning Platform

Nền tảng hỗ trợ học Lịch sử Việt Nam với AI (Gemini), bài trắc nghiệm, quản lý chat history, và text ingestion từ PDF. Repo đã chuẩn hoá kiến trúc nhiều lớp và test theo 6-phase workflow.

## 📋 Tổng quan

ViHis là một nền tảng học tập lịch sử Việt Nam được xây dựng với:
- **Backend**: ASP.NET 8 Web API với kiến trúc Clean Architecture
- **Frontend**: React 18 + Vite + TypeScript
- **Database**: MongoDB (MongoDB Atlas)
- **AI**: Google Gemini API (cho Q&A và quiz generation)
- **Authentication**: JWT (HMAC-SHA256)
- **Testing**: xUnit + FluentAssertions với 165 test cases

## 🚀 Features

### 1. **AI_QA** - AI Question & Answer
Hỏi đáp lịch sử Việt Nam bằng AI với MongoDB RAG (Retrieval-Augmented Generation):
- **Endpoint**: `POST /api/v1/ai/ask`
- **Tính năng**: 
  - Hỏi đáp bằng tiếng Việt, English, French
  - MongoDB RAG retrieval cho context
  - Web fallback khi không có context trong database
  - Multi-language support
- **Service**: `GeminiStudyService` với `KWideRetriever`
- **Test Cases**: 40 tests (23 passed, 17 failed do Gemini API rate limit 429)

### 2. **AUTH_JWT** - Authentication & Authorization
Hệ thống đăng ký, đăng nhập và quản lý người dùng với JWT:
- **Endpoints**:
  - `POST /api/v1/auth/register` - Đăng ký user mới
  - `POST /api/v1/auth/login` - Đăng nhập và nhận JWT token
  - `GET /api/v1/auth/me` - Lấy thông tin user hiện tại (cần JWT)
  - `POST /api/v1/auth/change-password` - Đổi mật khẩu (cần JWT)
- **Service**: `JwtService` (generate token, validate token, hash password)
- **Test Cases**: 23 tests (Unit: 7, Integration: 16)
- **Test Focus**: JWT claims, expiry, issuer, audience, duplicate registration, invalid credentials

### 3. **GEN_QUIZ** - Quiz Generation & Management
Tạo và quản lý bài trắc nghiệm với AI:
- **Endpoints**:
  - `POST /api/v1/quiz/create` - Tạo quiz mới (MCQ/Essay)
  - `GET /api/v1/quiz/{quizId}` - Lấy thông tin quiz
  - `POST /api/v1/quiz/submit` - Nộp bài và tính điểm
  - `GET /api/v1/quiz/my-quizzes` - Danh sách quiz của user
  - `GET /api/v1/quiz/my-attempts` - Lịch sử làm bài của user
- **Service**: `QuizService` → `QuizGenerationService` (Gemini API)
- **Test Cases**: 28 tests (Unit: 17, Integration: 11)
- **Test Focus**: MCQ auto-scoring, essay không chấm điểm, stable question order, multi-submit

### 4. **CHAT** - Chat History Management
Quản lý lịch sử chat và messages:
- **Endpoints**:
  - `GET /api/v1/chat/boxes` - Lấy danh sách chat boxes (theo userId hoặc machineId)
  - `POST /api/v1/chat/history` - Lưu/update chat history
  - `GET /api/v1/chat/history/{boxId}` - Lấy lịch sử chat của một box
  - `PUT /api/v1/chat/history/{boxId}/name` - Đổi tên chat box
  - `DELETE /api/v1/chat/history/{boxId}` - Xóa chat box và messages (cascade delete)
- **Architecture**: Controller → MongoDB (trực tiếp, không có Service Layer)
- **Test Cases**: 25 tests (Integration only)
- **Test Focus**: GetChatBoxes, SaveHistory (create/update), cascade delete, sorting, idempotent delete

### 5. **TEXT_INGEST** - PDF Text Ingestion
Ingest PDF files, extract text, chunk và generate embeddings:
- **Endpoints**:
  - `POST /api/v1/ingest/preview` - Preview PDF (extract 10 chunks đầu, không lưu DB)
  - `POST /api/v1/ingest/pdf` - Ingest PDF và lưu vào MongoDB (Source + Chunks với embeddings)
  - `GET /api/v1/ingest/chunks` - Lấy tất cả chunks (có pagination, filter by sourceId)
  - `GET /api/v1/ingest/sources` - Lấy tất cả sources (có pagination)
  - `GET /api/v1/ingest/source/{id}` - Lấy source và chunks của nó
- **Services**: `FallbackAIngestor`, `PdfTextExtractor`, `TextNormalizer`, `HeaderFooterDetector`, `SentenceTokenizer`, `ChunkPack`
- **Test Cases**: 49 tests (Unit: 35, Integration: 14)
- **Test Focus**: PDF extraction, text normalization, header/footer detection, chunking, embedding generation

### 6. **EMBEDDINGS** - Embedding Management (Admin)
Quản lý embeddings cho chunks để hỗ trợ Vector Search:
- **Endpoints**:
  - `POST /api/v1/admin/embeddings/generate-all` - Generate embeddings cho chunks chưa có
  - `GET /api/v1/admin/embeddings/status` - Check embedding status (% chunks có embeddings)
  - `DELETE /api/v1/admin/embeddings/all` - Xóa tất cả embeddings (để regenerate)
- **Tác dụng**: 
  - **Enable Vector Search**: Chuyển đổi text → vectors (768 dimensions) để hỗ trợ semantic search trong RAG
  - **Improve AI_QA**: AI_QA sử dụng vector search để tìm chunks liên quan hơn, trả lời chính xác hơn
  - **Migration Tool**: Generate embeddings cho existing chunks (backfill/migration)
  - **Admin Management**: Check status, generate theo batch, delete/regenerate
- **Technology**: Gemini Embedding API (text-embedding-004), MongoDB Vector Search Index
- **Workflow**: TEXT_INGEST → Chunks → EMBEDDINGS (generate) → AI_QA (vector search)
- **Status**: ⚠️ Chưa có test suite

### 7. **ADMIN** - Admin Utilities
Utility endpoints cho MongoDB:
- **Endpoints**:
  - `GET /api/v1/admin/mongo/ping` - Ping MongoDB
  - `GET /api/v1/admin/mongo/info` - MongoDB database info
- **Status**: ⚠️ Chưa có test suite

## 🛠️ Tech Stack

### Backend
- **Framework**: ASP.NET 8 Web API
- **Architecture**: Clean Architecture (Api → Application → Domain ← Infrastructure)
- **Database**: MongoDB (MongoDB.Driver, MongoDB Atlas)
- **Authentication**: JWT (HMAC-SHA256)
- **AI**: Google Gemini API (gemini-2.5-flash, gemini-2.0-flash-exp)
- **PDF Processing**: PdfPig library
- **Testing**: xUnit + FluentAssertions + Coverlet (code coverage)

### Frontend
- **Framework**: React 18
- **Build Tool**: Vite
- **Language**: TypeScript
- **HTTP Client**: Axios

### Development Tools
- **Code Coverage**: Coverlet + ReportGenerator
- **Test Runner**: xUnit
- **Assertions**: FluentAssertions
- **Package Manager**: NuGet (.NET), npm (Frontend)

## 🧱 Architecture

### Runtime Flow
```
FrontEnd (React)
    ↓ HTTP Request
VietHistory.Api (Controllers)
    ↓ Service Calls
VietHistory.Application (Services/DTOs)
    ↓ Business Logic
VietHistory.Domain (Entities/Rules)
    ↓ Implementations
VietHistory.Infrastructure (Mongo/JWT/AI/Gemini)
    ↓ External Calls
MongoDB | Gemini API
    ↓ Response
Back to API → FrontEnd
```

### Dependency Direction
- **Api** → **Application** → **Domain** ← **Infrastructure**
- Infrastructure implements Domain interfaces
- Domain không phụ thuộc vào Infrastructure
- Không trả Domain Entity trực tiếp; dùng DTO

### Layer Responsibilities

**VietHistory.Api** (Controllers):
- HTTP request/response handling
- Route definitions
- Input validation
- Authentication/Authorization

**VietHistory.Application** (Services/DTOs):
- Business logic orchestration
- Service interfaces
- Data Transfer Objects (DTOs)
- Input/Output contracts

**VietHistory.Domain** (Entities/Common):
- Domain entities (POCOs)
- Business rules
- Domain interfaces
- Value objects

**VietHistory.Infrastructure** (Mongo/JWT/AI):
- MongoDB implementation
- JWT implementation
- Gemini API integration
- External service clients
- Repositories implementation

## 📁 Folder Structure

```
ViHis/
├── BackEnd/
│   ├── VietHistory.Api/                    # Controllers, Program.cs, Startup
│   │   └── Controllers/
│   │       ├── AuthController.cs           # AUTH_JWT
│   │       ├── QuizController.cs          # GEN_QUIZ
│   │       ├── AiController.cs            # AI_QA
│   │       ├── ChatController.cs          # CHAT
│   │       ├── IngestController.cs        # TEXT_INGEST
│   │       ├── EmbeddingsController.cs    # EMBEDDINGS (admin)
│   │       └── AdminController.cs         # ADMIN utilities
│   ├── VietHistory.Application/            # Service interfaces, DTOs
│   │   └── DTOs/                          # Data Transfer Objects
│   ├── VietHistory.Domain/                 # Entities, Interfaces
│   │   ├── Entities/                      # Domain entities (POCOs)
│   │   ├── Common/                        # BaseEntity, Interfaces
│   │   └── Repositories/                  # Repository interfaces
│   ├── VietHistory.Infrastructure/        # MongoDB, JWT, AI implementations
│   │   ├── Mongo/                         # MongoDB context, repositories
│   │   ├── Services/                      # Business services
│   │   │   ├── JwtService.cs             # JWT generation/validation
│   │   │   ├── QuizService.cs            # Quiz business logic
│   │   │   ├── QuizGenerationService.cs  # AI quiz generation
│   │   │   ├── GeminiStudyService.cs     # AI Q&A service
│   │   │   ├── EmbeddingService.cs       # Embedding generation
│   │   │   └── TextIngest/               # PDF ingestion services
│   │   └── Services/Gemini/              # Gemini API clients
│   └── VietHistory.AI.Tests/              # xUnit test suite
│       ├── AUTH_JWT_UnitTests.cs          # 7 unit tests
│       ├── AUTH_JWT_IntegrationTests.cs    # 16 integration tests
│       ├── GEN_QUIZ_UnitTests.cs          # 17 unit tests
│       ├── GEN_QUIZ_IntegrationTests.cs    # 11 integration tests
│       ├── AI_QA_IntegrationTests.cs       # 9 integration tests
│       ├── AI_QA_RealTests.cs            # 31 real API tests
│       ├── CHAT_IntegrationTests.cs       # 25 integration tests
│       ├── TEXT_INGEST_UnitTests.cs       # 35 unit tests
│       └── TEXT_INGEST_IntegrationTests.cs # 14 integration tests
├── FrontEnd/                              # React + Vite + TypeScript
│   ├── src/
│   └── package.json
├── prompts/
│   ├── MASTER_ARCH_PROMPT_VIHIS.md       # Comprehensive architecture prompt
│   ├── AI_PROMPT_PLAYBOOK.md             # 6-phase testing playbook
│   └── results/                          # Phase results documentation
│       ├── AUTH_JWT/                     # Phase 1-6 results
│       ├── GEN_QUIZ/                     # Phase 1-6 results
│       ├── AI_QA/                       # Phase 1-6 results
│       ├── CHAT/                        # Phase 1-6 results
│       └── TEXT_INGEST/                 # Phase 1-6 results
├── tests/                                # Test plans
│   ├── CHAT_TEST_PLAN.md
│   └── TEXT_INGEST_TEST_PLAN.md
└── README.md
```

## 🔌 API Endpoints

### Authentication (AUTH_JWT)
- `POST /api/v1/auth/register` - Register new user
- `POST /api/v1/auth/login` - Login and get JWT token
- `GET /api/v1/auth/me` - Get current user info (requires JWT)
- `POST /api/v1/auth/change-password` - Change password (requires JWT)

### Quiz (GEN_QUIZ)
- `POST /api/v1/quiz/create` - Create new quiz (MCQ/Essay)
- `GET /api/v1/quiz/{quizId}` - Get quiz details
- `POST /api/v1/quiz/submit` - Submit quiz attempt
- `GET /api/v1/quiz/my-quizzes` - Get user's quizzes
- `GET /api/v1/quiz/my-attempts` - Get user's quiz attempts

### AI Q&A (AI_QA)
- `POST /api/v1/ai/ask` - Ask question (Vietnamese/English/French)

### Chat (CHAT)
- `GET /api/v1/chat/boxes` - Get chat boxes (by userId or machineId)
- `POST /api/v1/chat/history` - Save/update chat history
- `GET /api/v1/chat/history/{boxId}` - Get chat history
- `PUT /api/v1/chat/history/{boxId}/name` - Rename chat box
- `DELETE /api/v1/chat/history/{boxId}` - Delete chat box

### Text Ingestion (TEXT_INGEST)
- `POST /api/v1/ingest/preview` - Preview PDF (10 chunks, no DB save)
- `POST /api/v1/ingest/pdf` - Ingest PDF and save to MongoDB
- `GET /api/v1/ingest/chunks` - Get all chunks (pagination, filter by sourceId)
- `GET /api/v1/ingest/sources` - Get all sources (pagination)
- `GET /api/v1/ingest/source/{id}` - Get source and its chunks

### Embeddings (EMBEDDINGS) - Admin
- `POST /api/v1/admin/embeddings/generate-all` - Generate embeddings for chunks
- `GET /api/v1/admin/embeddings/status` - Check embedding status
- `DELETE /api/v1/admin/embeddings/all` - Delete all embeddings

### Admin (ADMIN)
- `GET /api/v1/admin/mongo/ping` - Ping MongoDB
- `GET /api/v1/admin/mongo/info` - Get MongoDB database info

**Chi tiết API contracts**: Xem `prompts/MASTER_ARCH_PROMPT_VIHIS.md`

## ⚙️ Setup & Run

### Prerequisites
- .NET 8 SDK
- Node.js 18+ (cho Frontend)
- MongoDB Atlas account (hoặc local MongoDB)
- Google Gemini API key (cho AI features)

### Backend Setup

1. **Clone repository**:
```bash
git clone https://github.com/binz0209/ViHis.git
cd ViHis
```

2. **Configure environment variables**:

Tạo `BackEnd/VietHistory.Api/appsettings.Development.json`:
```json
{
  "Mongo": {
    "ConnectionString": "mongodb+srv://user:password@cluster.mongodb.net/?retryWrites=true&w=majority",
    "Database": "vihis_dev"
  },
  "JWT": {
    "Key": "your-256-bit-secret-key-here-minimum-32-characters",
    "Issuer": "ViHis",
    "Audience": "ViHisUsers",
    "ExpMin": 60
  },
  "Gemini": {
    "ApiKey": "your-gemini-api-key",
    "Model": "gemini-2.5-flash"
  }
}
```

3. **Run backend**:
```bash
cd BackEnd/VietHistory.Api
dotnet restore
dotnet run
```

API sẽ chạy tại: `https://localhost:5001` (HTTPS) hoặc `http://localhost:5000` (HTTP)

### Frontend Setup

1. **Install dependencies**:
```bash
cd FrontEnd
npm install
```

2. **Configure environment**:

Tạo `.env` file:
```
VITE_API_BASE_URL=https://localhost:5001
```

3. **Run frontend**:
```bash
npm run dev
```

Frontend sẽ chạy tại: `http://localhost:5173`

## 🧪 Testing & Coverage

### Test Statistics

**Total Test Cases**: 165 tests

| Feature | Test Files | Unit Tests | Integration Tests | Total | Status |
|---------|-----------|------------|-------------------|-------|--------|
| **AUTH_JWT** | 2 files | 7 | 16 | 23 | ✅ 100% pass |
| **GEN_QUIZ** | 2 files | 17 | 11 | 28 | ✅ 100% pass |
| **AI_QA** | 2 files | 0 | 40 | 40 | ⚠️ 23/40 pass (17 fail do 429 rate limit) |
| **CHAT** | 1 file | 0 | 25 | 25 | ✅ 100% pass |
| **TEXT_INGEST** | 2 files | 35 | 14 | 49 | ✅ 100% pass |

**Test Summary**:
- ✅ **Passed**: 150/167 tests (89.8%)
- ❌ **Failed**: 17/167 tests (10.2%) - All from AI_QA do Gemini API rate limit (429)
- ⏱️ **Duration**: ~3-4 minutes

### Test Naming Convention

- **Format**: `TCxx_Given_When_Then`
- **Example**: `TC01_GetChatBoxes_WithUserId_ReturnsBoxes`

### Test Traits

All tests use standard traits:
- `[Trait("Feature", "AUTH_JWT|GEN_QUIZ|AI_QA|CHAT|TEXT_INGEST")]`
- `[Trait("Category", "HappyPath|EdgeCase|ErrorHandling|Integration")]`
- `[Trait("Priority", "P0|P1|P2")]`
- `[Trait("Integration", "Real|Mocked")]`

### Test Pattern: AAA (Arrange-Act-Assert)

All test code follows **AAA pattern** (Arrange-Act-Assert):

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

### Running Tests

#### Run all tests:
```bash
cd BackEnd
dotnet test
```

#### Run tests by feature:
```bash
cd BackEnd
dotnet test --filter "Feature=AUTH_JWT"
dotnet test --filter "Feature=GEN_QUIZ"
dotnet test --filter "Feature=AI_QA"
dotnet test --filter "Feature=CHAT"
dotnet test --filter "Feature=TEXT_INGEST"
```

#### Run all features:
```bash
dotnet test --filter "Feature=AUTH_JWT|GEN_QUIZ|AI_QA|CHAT|TEXT_INGEST"
```

#### Run with verbose output:
```bash
dotnet test --logger:"console;verbosity=detailed"
```

### Code Coverage

#### Generate Coverage Report:

**Step 1**: Run tests with coverage collection:
```bash
cd BackEnd
dotnet test --collect:"XPlat Code Coverage"
```

**Step 2**: Generate HTML report:
```bash
# Install ReportGenerator if not already installed
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
export PATH="$PATH:$HOME/.dotnet/tools"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport-final" "-reporttypes:Html;HtmlSummary"

# Open report
open coveragereport-final/index.html
```

**Coverage Status**:
- **Line Coverage**: 20.9% (145/692 covered lines)
- **Branch Coverage**: 28.2% (70/248 branches)
- **Note**: Coverage thấp một phần do Domain/Application POCOs không nên test (best practice), một phần do EMBEDDINGS và ADMIN chưa có test suite.

**Target Coverage**: ≥85% cho code có business logic (exclude POCOs)

## 🧭 Development Guidelines

### Code Style
- Controllers mỏng (thin controllers) - chỉ handle HTTP, không có business logic
- Business logic ở Application layer (Services)
- IO operations ở Infrastructure layer
- Tên biến/hàm rõ nghĩa, self-documenting
- Guard clauses cho input validation
- Không hardcode secrets - dùng environment variables

### Testing Guidelines
- Tuân theo 6-phase workflow: Analyze → Design → Code → Debug → Optimize → Docs
- Phase 2 (Design) dùng **Given-When-Then (GWT)** cho test matrix
- Phase 3 (Code) dùng **Arrange-Act-Assert (AAA)** cho test implementation
- Test method naming: `TCxx_Given_When_Then`
- Tất cả test methods phải có AAA structure rõ ràng
- Khi đổi hành vi, bổ sung/điều chỉnh test
- Đảm bảo `dotnet test` pass trước khi commit

### Git Workflow
- Commit messages rõ ràng, mô tả thay đổi
- Pull trước khi push (`git pull --rebase`)
- Test pass trước khi push

## 📚 Documentation

### Architecture & API
- **Master Architecture Prompt**: `prompts/MASTER_ARCH_PROMPT_VIHIS.md`
  - Comprehensive architecture guidelines
  - API contracts và data flow
  - Tech stack details
  - Design patterns

### Testing Playbook
- **6-Phase Testing Playbook**: `prompts/AI_PROMPT_PLAYBOOK.md`
  - Phase 1: Analysis & Selection
  - Phase 2: Test Case Design (GWT)
  - Phase 3: Test Code Generation (AAA)
  - Phase 4: Run & Debug
  - Phase 5: Optimize & Coverage
  - Phase 6: Documentation
  - Feature guides cho tất cả features

### Phase Results
- `prompts/results/AUTH_JWT/` - Phase 1-6 results cho AUTH_JWT
- `prompts/results/GEN_QUIZ/` - Phase 1-6 results cho GEN_QUIZ
- `prompts/results/AI_QA/` - Phase 1-6 results cho AI_QA
- `prompts/results/CHAT/` - Phase 1-6 results cho CHAT
- `prompts/results/TEXT_INGEST/` - Phase 1-6 results cho TEXT_INGEST

### Test Plans
- `tests/CHAT_TEST_PLAN.md` - Test plan cho CHAT feature
- `tests/TEXT_INGEST_TEST_PLAN.md` - Test plan cho TEXT_INGEST feature

## 🎯 Known Issues & Limitations

### AI_QA Rate Limiting
- **Issue**: Gemini API có rate limit (429 Too Many Requests)
- **Impact**: 17/40 AI_QA tests có thể fail do rate limit
- **Workaround**: Chạy AI_QA tests riêng với delay, hoặc accept failed tests do rate limit
- **Long-term**: Implement retry with exponential backoff

### Coverage
- **Current**: 20.9% line coverage
- **Reason**: 
  - Domain/Application POCOs không nên test (best practice) → Đúng
  - EMBEDDINGS và ADMIN controllers chưa có test suite → Cần thêm
- **Action**: 
  - Exclude POCOs từ coverage calculation
  - Tạo test suite cho EMBEDDINGS và ADMIN

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Follow development guidelines (6-phase testing workflow)
4. Ensure all tests pass (`dotnet test`)
5. Commit changes (`git commit -m 'Add some AmazingFeature'`)
6. Push to branch (`git push origin feature/AmazingFeature`)
7. Open Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

**Last updated**: 2025-11-01

**Project Status**: ✅ Active Development

**Test Status**: ✅ 165 test cases, 89.8% pass rate

**Coverage Status**: ⚠️ 20.9% (target: ≥85% for business logic code)
