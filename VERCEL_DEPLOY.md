# 🚀 Deploy ViHis lên Vercel

## Bước 1: Chuẩn bị Backend

Backend hiện tại chạy ở `localhost:5000`. Bạn cần host backend trước:

### Option A: Deploy backend lên Railway (Recommended)
```bash
cd BackEnd
# Railway sẽ tự detect ASP.NET Core và deploy
railway up
```

### Option B: Deploy backend lên Render
1. Tạo account tại https://render.com
2. Tạo new Web Service
3. Connect GitHub repo
4. Deploy từ BackEnd folder

### Lưu ý:
Sau khi deploy backend, lấy URL (ví dụ: `https://viet-history-api.onrender.com`)

## Bước 2: Deploy Frontend lên Vercel

### 2.1. Push code lên GitHub

```bash
git add .
git commit -m "Ready for Vercel deployment"
git push origin main
```

### 2.2. Deploy trên Vercel

1. **Mở https://vercel.com và login**

2. **Import project:**
   - Click "Add New..." → "Project"
   - Chọn repository GitHub của bạn

3. **Configure Project:**
   ```
   Framework Preset: Vite
   Root Directory: FrontEnd (hoặc để trống nếu FrontEnd là root)
   Build Command: npm run build
   Output Directory: dist
   ```

4. **Add Environment Variables:**
   Click "Environment Variables" và add:
   ```
   Name: VITE_API_URL
   Value: https://your-backend-url.com
   ```
   (Ví dụ: `https://viet-history-api.onrender.com`)

5. **Deploy:**
   Click "Deploy"

## Bước 3: Cấu hình CORS trên Backend

Backend cần cho phép domain Vercel. Mở `BackEnd/VietHistory.Api/appsettings.json`:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173",
      "https://your-frontend.vercel.app"
    ]
  }
}
```

## Bước 4: Test

1. Mở URL Vercel của bạn (ví dụ: `https://vihis.vercel.app`)
2. Test chat với AI
3. Test đăng nhập
4. Test tạo chat box mới

## Troubleshooting

### Lỗi: "Failed to fetch"
- Kiểm tra `VITE_API_URL` trong Vercel Environment Variables
- Đảm bảo backend đang chạy và accessible

### Lỗi: CORS
- Backend cần thêm domain Vercel vào `AllowedOrigins`
- Rebuild và redeploy backend

### Lỗi: Build failed
```bash
cd FrontEnd
npm install
npm run build
```

## Quick Start

**Nhanh nhất để test:**

1. Deploy backend lên Railway (miễn phí)
2. Lấy backend URL
3. Deploy frontend lên Vercel:
   ```bash
   cd FrontEnd
   npx vercel
   ```
   - Nhập `VITE_API_URL` khi được hỏi
4. Done! 🎉

## Liên kết hữu ích

- Vercel: https://vercel.com
- Railway: https://railway.app
- Render: https://render.com


