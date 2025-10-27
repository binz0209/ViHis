# VietHistory Frontend Guide

## 📁 Cấu trúc dự án Frontend

```
FrontEnd/
├── public/                    # Static files
├── src/
│   ├── components/           # React components
│   │   ├── ChatBox.tsx      # Component chatbox chính
│   │   └── ChatBox.css      # Styles cho chatbox
│   ├── services/             # API services
│   │   └── api.ts           # Axios client và API functions
│   ├── App.tsx              # Main app component
│   ├── App.css              # App styles
│   ├── main.tsx             # Entry point
│   ├── index.css            # Global styles
│   └── vite-env.d.ts        # TypeScript type definitions
├── index.html                # HTML template
├── package.json              # Dependencies
├── tsconfig.json             # TypeScript config
├── vite.config.ts            # Vite config
└── README.md                 # Frontend README
```

## 🚀 Quick Start

### Chạy ứng dụng

**Option 1: Sử dụng script tự động (Windows)**
```bash
start-dev.bat
```

**Option 2: Chạy manual**

Terminal 1 - Backend:
```bash
cd BackEnd
dotnet run --project VietHistory.Api
```

Terminal 2 - Frontend:
```bash
cd FrontEnd
npm install
npm run dev
```

### Mở trình duyệt
- Frontend: http://localhost:3000
- Backend API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

## 🎨 Tính năng Frontend

### ChatBox Component
- **Input**: Textarea cho phép người dùng nhập câu hỏi
- **Send Button**: Nút gửi câu hỏi (hoặc nhấn Enter)
- **Messages**: Hiển thị lịch sử chat với user/assistant
- **Loading State**: Hiển thị dots loading khi đang xử lý
- **Welcome Screen**: Hiển thị ví dụ câu hỏi khi chưa có tin nhắn
- **Auto Scroll**: Tự động scroll đến tin nhắn mới nhất

### UI/UX Features
- ✅ Responsive design (mobile friendly)
- ✅ Gradient background đẹp mắt
- ✅ Smooth animations
- ✅ Loading indicators
- ✅ Error handling
- ✅ Auto-scroll to bottom
- ✅ Keyboard shortcuts (Enter to send)

## 🔌 API Integration

### Endpoint sử dụng
```
POST /api/v1/ai/ask
```

### Request Body
```typescript
{
  question: string,      // Câu hỏi của người dùng
  language?: string,     // Ngôn ngữ trả lời (mặc định: 'vi')
  maxContext?: number    // Số lượng context chunks (mặc định: 5)
}
```

### Response
```typescript
{
  answer: string,        // Câu trả lời từ AI
  model: string,         // Model name (ví dụ: 'gemini-2.5-flash')
  costUsd?: number       // Chi phí (nếu có)
}
```

## 📝 File quan trọng

### `src/services/api.ts`
File này chứa logic kết nối với backend API:
- `sendMessage()`: Gửi câu hỏi và nhận câu trả lời
- Cấu hình axios client
- Xử lý errors

### `src/components/ChatBox.tsx`
Component chính của chatbox:
- Quản lý state (messages, input, loading)
- Xử lý user interactions
- Call API khi gửi tin nhắn

### `vite.config.ts`
Cấu hình Vite:
- Proxy API calls từ `/api` đến backend port 5000
- Port frontend: 3000

## 🎯 Ví dụ câu hỏi

Bạn có thể thử các câu hỏi sau:

```
"Chiến thắng Điện Biên Phủ diễn ra năm nào?"
"Kể tôi nghe về cuộc khởi nghĩa Hai Bà Trưng"
"Ai là vị vua đầu tiên của triều Nguyễn?"
"Hãy kể tôi nghe về chiến dịch mùa xuân năm 1975"
"Bác Hồ sinh ngày tháng năm nào?"
```

## 🛠️ Customization

### Thay đổi API URL
Tạo file `.env` trong `FrontEnd/`:
```env
VITE_API_URL=http://localhost:5000
```

### Thay đổi màu sắc
Sửa file `src/components/ChatBox.css`:
```css
/* Gradient background */
.message.user .message-bubble {
  background: linear-gradient(135deg, YOUR_COLOR_1 0%, YOUR_COLOR_2 100%);
}
```

### Thay đổi port frontend
Sửa file `vite.config.ts`:
```typescript
server: {
  port: 3001,  // Đổi port ở đây
  // ...
}
```

## 📦 Dependencies

### Production
- `react`: UI library
- `react-dom`: React DOM rendering
- `axios`: HTTP client cho API calls

### Development
- `vite`: Build tool và dev server
- `typescript`: TypeScript compiler
- `@vitejs/plugin-react`: Vite plugin cho React

## 🐛 Troubleshooting

### Lỗi "Cannot connect to API"
- Kiểm tra backend đang chạy tại port 5000
- Kiểm tra file `.env` có đúng URL không
- Kiểm tra CORS trong backend

### Lỗi khi npm install
- Xóa `node_modules` và `package-lock.json`
- Chạy `npm install` lại

### Lỗi TypeScript
- Kiểm tra file `tsconfig.json`
- Đảm bảo đã cài đặt TypeScript

## 📚 Tài liệu thêm

- [React Documentation](https://react.dev)
- [Vite Documentation](https://vitejs.dev)
- [Axios Documentation](https://axios-http.com)


