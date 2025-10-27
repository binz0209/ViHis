# ⚡ Quick Deploy ViHis lên Vercel

## 🎯 Deployment Summary

**Build Status:** ✅ **SUCCESS**  
**Build Output:** `dist/` folder created  
**Total Size:** ~192KB (gzipped: ~68KB)

## 📋 Deploy Steps

### 1. Deploy Backend (Chọn 1 option)

#### Option A: Railway (Recommended - Free)
```bash
cd BackEnd
# Install Railway CLI
npm install -g @railway/cli

# Login
railway login

# Deploy
railway up
```

#### Option B: Render (Free tier)
1. Mở https://render.com
2. New → Web Service
3. Connect GitHub repo
4. Build: `cd BackEnd/VietHistory.Api && dotnet publish -c Release`
5. Start: `dotnet VietHistory.Api.dll`

#### Option C: Keep local for now
Backend có thể chạy local và frontend Vercel vẫn connect được qua ngrok hoặc local tunnel

### 2. Deploy Frontend lên Vercel

#### Option A: Via Vercel CLI (Fastest)
```bash
cd FrontEnd

# Login Vercel
npx vercel login

# Deploy
npx vercel
```
Follow prompts:
- Link to existing project? → No
- Set up? → Yes
- Which scope? → Your account
- Link to existing project? → No
- What's your project's name? → vihis
- In which directory is your code located? → ./
- Want to override the settings? → Yes
- Development Command: `npm run dev`
- Build Command: `npm run build`
- Output Directory: `dist`
- Install Command: `npm install`

Add environment variable:
```bash
npx vercel env add VITE_API_URL
# Enter: https://your-backend-url.com
```

#### Option B: Via Vercel Dashboard
1. Mở https://vercel.com/new
2. Import từ GitHub
3. Config:
   - Root Directory: `FrontEnd`
   - Build Command: `npm run build`
   - Output Directory: `dist`
4. Environment Variables:
   ```
   VITE_API_URL = http://localhost:5000  (for dev)
   hoặc
   VITE_API_URL = https://your-backend-url.com  (for prod)
   ```
5. Deploy!

### 3. Update Backend CORS

Mở `BackEnd/VietHistory.Api/appsettings.json`:
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173",
      "https://your-app.vercel.app"
    ]
  }
}
```

Rebuild và redeploy backend.

## ✅ Checklist

- [ ] Backend deployed và running
- [ ] Backend URL accessible (test với Postman)
- [ ] CORS configured cho Vercel domain
- [ ] Frontend build successful (`npm run build`)
- [ ] Vite_API_URL set trong Vercel
- [ ] Deploy successful
- [ ] Test trên production URL

## 🔗 Useful Commands

```bash
# Build frontend locally
cd FrontEnd && npm run build

# Test production build locally
cd FrontEnd && npm run preview

# Check Vercel deployment
vercel ls

# View logs
vercel logs
```

## 🎉 Sau khi deploy thành công

Frontend URL: `https://your-app.vercel.app`  
Backend URL: `https://your-backend-url.com`

Test các features:
- ✅ Chat với AI
- ✅ Tạo chat box mới
- ✅ Đăng nhập/đăng xuất
- ✅ Lưu lịch sử chat

## 🐛 Troubleshooting

**Build fails:**
```bash
cd FrontEnd
rm -rf node_modules dist
npm install
npm run build
```

**API not working:**
- Check `VITE_API_URL` environment variable
- Check backend CORS settings
- Check backend logs

**CORS errors:**
- Add Vercel domain vào `AllowedOrigins` trong backend
- Rebuild và redeploy backend

## 📞 Support

Nếu gặp vấn đề:
1. Check Vercel deployment logs
2. Check browser console
3. Check backend logs
4. Test API endpoints với Postman

---

**Ready to deploy?** Run:
```bash
cd FrontEnd && npx vercel
```


