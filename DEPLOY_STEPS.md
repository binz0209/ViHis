# 🚀 Deploy ViHis lên Production

## ✅ Đã hoàn thành

### 1. CORS đã được set
Backend (`appsettings.json`) đã cho phép:
```
https://vihis.vercel.app
```

### 2. Frontend đã deploy
Preview URL: https://vihis-8pd47ko7p-binzdapoet0209-7462s-projects.vercel.app

## 🔧 Tiếp theo - Setup Vercel Production

### Bước 1: Add Environment Variable

1. Mở https://vercel.com/binzdapoet0209-7462s-projects/vihis
2. Vào **Settings** → **Environment Variables**
3. Click **Add New**
4. Thêm biến:
   ```
   Name: VITE_API_URL
   Value: http://localhost:5000
   ```
5. Chọn environments: ✅ Production, ✅ Preview, ✅ Development
6. Click **Save**

### Bước 2: Deploy Production

```bash
cd FrontEnd
npx vercel --prod
```

Hoặc qua dashboard:
1. Vào tab **Deployments**
2. Chọn deployment mới nhất
3. Click **...** → **Promote to Production**

### Bước 3: Test Production URL

Production URL: https://vihis.vercel.app

Test:
- ✅ Chat với AI
- ✅ Tạo chat box
- ✅ Login/logout
- ✅ Mobile responsive

## ⚠️ Backend Setup

Backend hiện chạy ở `localhost:5000`. Bạn có 2 options:

### Option A: Deploy Backend lên Production

**Railway (Recommended):**
```bash
cd BackEnd
npm install -g @railway/cli
railway login
railway up
```

Sau đó:
1. Lấy Railway URL (ví dụ: `https://vihis-api.up.railway.app`)
2. Update `VITE_API_URL` trong Vercel = Railway URL
3. Redeploy frontend

**Render:**
1. https://render.com
2. New Web Service
3. Connect GitHub
4. Build: `dotnet build`
5. Start: `dotnet run`
6. Lấy URL và update Vercel env

### Option B: Sử dụng local backend

Frontend Vercel có thể connect tới localhost backend nếu:
- Dùng ngrok: `ngrok http 5000`
- Dùng cloudflare tunnel
- Hoặc expose backend local

---

**Frontend URL:** https://vihis.vercel.app  
**Backend:** Cần deploy hoặc expose local

