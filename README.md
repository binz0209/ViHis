# 🇻🇳 ViHis - Trợ lý Lịch sử Việt Nam

Chatbot thông minh giúp bạn tìm hiểu lịch sử Việt Nam với AI.

## 🌐 Live URLs

- **Production:** https://vihis.vercel.app
- **Backend API:** https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net
- **Swagger:** https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net/swagger

## ✨ Features

- ✅ **AI Chat:** Trò chuyện về lịch sử Việt Nam với AI thông minh
- ✅ **Context Aware:** AI nhớ lịch sử cuộc trò chuyện
- ✅ **Multi-box Chat:** Nhiều chat boxes, quản lý dễ dàng
- ✅ **Login System:** Đăng nhập, đăng ký với JWT
- ✅ **Mobile Responsive:** Drawer menu, full screen chat
- ✅ **Database:** MongoDB lưu trữ lịch sử chat
- ✅ **PDF Processing:** Xử lý tài liệu lịch sử từ PDF

## 🏗️ Tech Stack

### Frontend
- React + TypeScript
- Vite
- Axios
- Modern UI with glassmorphism

### Backend
- ASP.NET Core 8
- MongoDB
- Gemini AI (Google)
- JWT Authentication
- CORS configured

## 🚀 Local Development

### Prerequisites
- Node.js 18+
- .NET 8.0
- MongoDB (hoặc MongoDB Atlas)

### Backend
```bash
cd BackEnd/VietHistory.Api
dotnet restore
dotnet run
```

Backend chạy tại: `http://localhost:5000`

### Frontend
```bash
cd FrontEnd
npm install
npm run dev
```

Frontend chạy tại: `http://localhost:5173`

## 📱 Mobile Features

- **Header:** Thu nhỏ, compact
- **Drawer Menu:** 📁 button để toggle chat boxes
- **Full Screen Chat:** Không có sidebar trên mobile
- **Touch Friendly:** Buttons 44x44px minimum

## 🔐 Environment Variables

### Vercel (Frontend)
```
VITE_API_URL = https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net
```

### Azure (Backend)
```
ASPNETCORE_ENVIRONMENT = Production
Mongo__ConnectionString = <from appsettings.json>
Gemini__ApiKey = <from appsettings.json>
```

## 📂 Project Structure

```
ViHis/
├── BackEnd/                    # ASP.NET Core API
│   ├── VietHistory.Api/       # Main API project
│   ├── VietHistory.Application/  # Business logic
│   ├── VietHistory.Domain/    # Entities
│   └── VietHistory.Infrastructure/  # Services
├── FrontEnd/                   # React + Vite
│   ├── src/
│   │   ├── components/        # React components
│   │   ├── context/           # Auth context
│   │   ├── pages/             # Login page
│   │   ├── services/           # API calls
│   │   └── utils/             # Utilities
└── tests/                      # Test files
```

## 🎨 UI Design

- **Glassmorphism:** Semi-transparent panels với blur effect
- **Gradient Background:** Blue to purple gradient
- **Modern Typography:** Inter font family
- **Smooth Animations:** Fade in, slide transitions
- **Dark Theme:** Friendly colors

## 📊 API Endpoints

### Chat
- `POST /api/v1/ai/ask` - Ask AI question
- `GET /api/v1/chat/boxes` - Get chat boxes
- `POST /api/v1/chat/history` - Save messages
- `GET /api/v1/chat/history/{boxId}` - Get messages
- `PUT /api/v1/chat/history/{boxId}/name` - Rename box
- `DELETE /api/v1/chat/history/{boxId}` - Delete box

### Auth
- `POST /api/v1/auth/register` - Register
- `POST /api/v1/auth/login` - Login
- `GET /api/v1/auth/me` - Get user info

## 🐛 Troubleshooting

### CORS Error
Backend đã configured để allow Vercel domains.

### Network Error
Set `VITE_API_URL` trong Vercel environment variables.

### Build Failed
```bash
cd FrontEnd && npm install && npm run build
```

## 📝 License

MIT

## 👤 Author

ViHis Team

---

**Built with ❤️ for Vietnamese History Education**
