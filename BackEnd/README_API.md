# VietHistory API - Quick Start

## Chạy Backend

```bash
cd BackEnd
dotnet run --project VietHistory.Api
```

API sẽ chạy tại: **http://localhost:5000**

## Swagger UI

Truy cập: http://localhost:5000/swagger

## API Endpoints

### AI Chat
- `POST /api/v1/ai/ask` - Gửi câu hỏi và nhận câu trả lời từ AI

### Ingest (PDF)
- `POST /api/v1/ingest/preview` - Preview PDF (không lưu DB)
- `POST /api/v1/ingest/pdf` - Upload và ingest PDF vào MongoDB
- `GET /api/v1/ingest/chunks` - Lấy danh sách chunks
- `GET /api/v1/ingest/sources` - Lấy danh sách sources
- `GET /api/v1/ingest/source/{id}` - Lấy chi tiết source và chunks

### People
- `GET /api/v1/people` - Lấy danh sách người
- `POST /api/v1/people` - Tạo người mới
- `GET /api/v1/people/{id}` - Lấy chi tiết người
- `PUT /api/v1/people/{id}` - Cập nhật người

### Events
- `GET /api/v1/events` - Lấy danh sách sự kiện
- `POST /api/v1/events` - Tạo sự kiện mới
- `GET /api/v1/events/{id}` - Lấy chi tiết sự kiện
- `PUT /api/v1/events/{id}` - Cập nhật sự kiện

## CORS

CORS đã được cấu hình để cho phép frontend (http://localhost:3000) kết nối.

Xem chi tiết: [CORS_SETUP.md](./CORS_SETUP.md)

## Cấu hình

File: `appsettings.json`

- **MongoDB**: Connection string và database name
- **Gemini**: API key, model, temperature
- **CORS**: Allowed origins

## Environment Variables

```bash
# Gemini API
export GEMINI_API_KEY="your-key"

# MongoDB
export MONGODB_CONNECTION="your-connection-string"

# Google Custom Search
export GOOGLE_CSE_KEY="your-key"
export GOOGLE_CSE_CX="your-cx"
```

## Testing

### Test API với cURL

```bash
# Test AI endpoint
curl -X POST http://localhost:5000/api/v1/ai/ask \
  -H "Content-Type: application/json" \
  -d '{
    "question": "Chiến thắng Điện Biên Phủ diễn ra năm nào?",
    "language": "vi",
    "maxContext": 5
  }'
```

### Test với Postman

Import collection từ Swagger: http://localhost:5000/swagger/v1/swagger.json

## Hot Reload

API tự động reload khi có thay đổi trong Development mode.

```bash
dotnet watch run --project VietHistory.Api
```


