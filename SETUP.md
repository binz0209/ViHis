# Hướng dẫn chạy VietHistory

## Yêu cầu

- Node.js (v18 hoặc cao hơn)
- .NET 8 SDK
- MongoDB Atlas account

## Cấu trúc dự án

```
ViHis/
├── BackEnd/          # Backend API (.NET)
├── FrontEnd/         # Frontend (React + TypeScript)
└── README.md
```

## Chạy Backend

1. **Di chuyển vào thư mục Backend:**
```bash
cd BackEnd
```

2. **Kiểm tra và cài đặt dependencies:**
```bash
dotnet restore
```

3. **Chạy API:**
```bash
dotnet run --project VietHistory.Api
```

Backend sẽ chạy tại `http://localhost:5000` (hoặc port được cấu hình trong `launchSettings.json`)

## Chạy Frontend

1. **Mở terminal mới và di chuyển vào thư mục Frontend:**
```bash
cd FrontEnd
```

2. **Cài đặt dependencies:**
```bash
npm install
```

3. **Chạy frontend:**
```bash
npm run dev
```

Frontend sẽ chạy tại `http://localhost:3000`

## Sử dụng ứng dụng

1. Mở trình duyệt và truy cập `http://localhost:3000`
2. Nhập câu hỏi về lịch sử Việt Nam vào chatbox
3. Nhấn Enter hoặc click nút Send để gửi câu hỏi
4. Chờ AI trả lời dựa trên dữ liệu đã ingest

## Ví dụ câu hỏi

- "Chiến thắng Điện Biên Phủ diễn ra năm nào?"
- "Kể tôi nghe về cuộc khởi nghĩa Hai Bà Trưng"
- "Ai là vị vua đầu tiên của triều Nguyễn?"
- "Hãy kể tôi nghe về chiến dịch mùa xuân năm 1975"

## Tùy chỉnh

### Thay đổi API URL

Trong file `FrontEnd/.env`:
```
VITE_API_URL=http://localhost:5000
```

### Thay đổi port backend

Chỉnh sửa file `BackEnd/VietHistory.Api/Properties/launchSettings.json`

## Build Production

### Backend
```bash
cd BackEnd
dotnet publish -c Release
```

### Frontend
```bash
cd FrontEnd
npm run build
```

File production sẽ nằm trong `FrontEnd/dist/`

## Troubleshooting

### Lỗi kết nối API
- Đảm bảo backend đang chạy tại port 5000
- Kiểm tra file `.env` trong FrontEnd có đúng URL không

### Lỗi CORS
- Backend đã được cấu hình cho phép tất cả origins
- Kiểm tra file `Program.cs` trong BackEnd/VietHistory.Api

### Lỗi dependencies
- Chạy `npm install` lại trong FrontEnd
- Chạy `dotnet restore` lại trong BackEnd


