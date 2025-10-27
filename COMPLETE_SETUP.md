# 🎯 ViHis - Complete Setup Guide

## ✅ Hiện tại

- ✅ Frontend: Deployed trên Vercel
- ✅ Backend: Running trên Azure
- ✅ CORS: Đã update để allow Vercel domains
- ⏳ Environment Variable: Cần set

## 🚀 Quick Setup

### 1. Set Vercel Environment Variable

**Cách nhanh nhất - Vercel Dashboard:**

1. Mở: https://vercel.com/binzdapoet0209-7462s-projects/vihis/settings/environment-variables
2. Click **Add New**
3. Add:
   ```
   Name:  VITE_API_URL
   Value: https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net
   ```
4. Chọn: ✅ Production, ✅ Preview, ✅ Development
5. Click **Save**

### 2. Redeploy Frontend

**Option A - Dashboard:**
1. Vào https://vercel.com/binzdapoet0209-7462s-projects/vihis/deployments
2. Click **...** trên deployment mới nhất
3. Click **Redeploy**
4. ✅ Done!

**Option B - CLI:**
```bash
cd FrontEnd
npx vercel --prod
```

### 3. Deploy Backend với CORS mới

Backend cần rebuild với CORS mới. Có 2 cách:

**Cách 1 - Azure Portal:**
1. Mở Azure Portal
2. Tìm App Service: `vihisprj`
3. **Deployment Center** → **Sync**
4. Hoặc trigger build từ GitHub/DevOps

**Cách 2 - Git Push:**
```bash
cd BackEnd
git add .
git commit -m "Update CORS for Vercel"
git push
# Azure sẽ auto deploy
```

## ✅ Verify Deployment

### Test Frontend:
1. Mở https://vihis.vercel.app
2. Check browser console (F12)
3. No CORS errors ✅

### Test Backend:
1. Mở https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net/swagger
2. Test API endpoints

### Test Integration:
1. Open https://vihis.vercel.app
2. Try to chat with AI
3. Should work without CORS errors ✅

## 🎯 URLs

- **Frontend Production:** https://vihis.vercel.app
- **Backend Azure:** https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net
- **Backend Swagger:** https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net/swagger

## 📋 Checklist

- [ ] Set `VITE_API_URL` trong Vercel
- [ ] Redeploy frontend
- [ ] Deploy backend với CORS mới
- [ ] Test frontend
- [ ] Test chat với AI
- [ ] Test login/logout
- [ ] Test mobile responsive

## 🐛 Common Issues

### 1. CORS Error

**Symptom:** Browser shows CORS error

**Fix:**
```bash
# Verify backend CORS
curl -H "Origin: https://vihis.vercel.app" -H "Access-Control-Request-Method: POST" -X OPTIONS https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net/api/v1/ai/ask -v
```

Should return:
```
Access-Control-Allow-Origin: https://vihis.vercel.app
```

### 2. API URL Not Found

**Symptom:** Network error when calling API

**Fix:**
- Check `VITE_API_URL` in Vercel
- Must start with `https://` for production
- Must NOT have trailing slash `/`

### 3. Environment Variable Not Applied

**Symptom:** Still using old API URL

**Fix:**
```bash
# Force rebuild
cd FrontEnd
npx vercel --prod --force
```

## 🎉 Done!

Sau khi hoàn thành tất cả bước:
- ✅ App live tại: https://vihis.vercel.app
- ✅ Fully functional
- ✅ Mobile responsive
- ✅ Backend API ready

---

**Need help?** Check logs:
- Frontend: Vercel dashboard → Deployments → View logs
- Backend: Azure Portal → App Service → Log stream

