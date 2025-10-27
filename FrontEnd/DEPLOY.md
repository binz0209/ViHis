# Deploy ViHis lên Vercel

## Yêu cầu
- GitHub account
- Vercel account (sign up tại https://vercel.com)

## Các bước deploy:

### 1. Đẩy code lên GitHub
```bash
cd FrontEnd
git init
git add .
git commit -m "Initial commit"
git remote add origin <your-github-repo-url>
git push -u origin main
```

### 2. Deploy lên Vercel

#### Option 1: Deploy từ GitHub (Recommended)
1. Mở https://vercel.com
2. Click "Add New..." → "Project"
3. Import repository từ GitHub
4. Configure:
   - Framework: Vite
   - Root Directory: FrontEnd (hoặc ./ nếu deploy toàn bộ repo)
   - Build Command: `npm run build`
   - Output Directory: `dist`
5. Add Environment Variable:
   - Key: `VITE_API_URL`
   - Value: URL của backend API (ví dụ: `http://localhost:5000` cho dev, hoặc backend URL cho production)
6. Click "Deploy"

#### Option 2: Deploy bằng Vercel CLI
```bash
npm install -g vercel
cd FrontEnd
vercel login
vercel
```

### 3. Cấu hình Environment Variables

Trong Vercel Dashboard:
1. Vào Project Settings → Environment Variables
2. Add:
   - `VITE_API_URL`: URL backend API của bạn
   - Ví dụ: `https://viet-history-api.herokuapp.com` hoặc `http://localhost:5000`

### 4. Backend API

Backend hiện tại đang chạy ở localhost:5000. Để frontend production hoạt động, cần:

**Option A: Deploy backend lên Vercel/Railway/Render**
- Backend ASP.NET Core có thể deploy lên:
  - Railway (khuyên dùng)
  - Render
  - Heroku
  - Azure

**Option B: Sử dụng local backend cho development**
- Dùng Vercel preview với `VITE_API_URL=http://localhost:5000` (chỉ hoạt động khi chạy local)

## Cấu trúc API

Frontend gọi các endpoint:
- `POST /api/v1/ai/ask` - Gửi câu hỏi AI
- `GET /api/v1/chat/boxes` - Lấy danh sách chat boxes
- `POST /api/v1/chat/history` - Lưu chat history
- `GET /api/v1/chat/history/:boxId` - Lấy messages của box

## Troubleshooting

### Lỗi CORS
Đảm bảo backend có cấu hình CORS cho domain Vercel:
```csharp
app.UseCors("AllowAll");
```

### Lỗi API not found
Kiểm tra `VITE_API_URL` đã đúng chưa trong Vercel environment variables.

### Build failed
```bash
cd FrontEnd
npm install
npm run build
```
Kiểm tra lỗi và fix trước khi deploy.


