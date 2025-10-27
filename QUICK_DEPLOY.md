# ⚡ Quick Deploy - ViHis

## 🎯 URLs
- **Frontend:** https://vihis.vercel.app  
- **Backend:** https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net

## ✅ Đã setup

### Backend CORS
✅ Program.cs - Cho phép tất cả `vercel.app` domains  
✅ appsettings.json - Thêm `https://vihis.vercel.app`

### Frontend
✅ Responsive mobile với drawer menu  
✅ Ready to connect với backend

## 🚀 3 BƯỚC ĐỂ HOÀN THÀNH

### Bước 1: Deploy Backend

```bash
cd BackEnd
git add .
git commit -m "Update CORS configuration"
git push
```

Azure sẽ tự động deploy.

### Bước 2: Set Environment Variable

**Vercel Dashboard:**
1. Mở: https://vercel.com/binzdapoet0209-7462s-projects/vihis/settings/environment-variables
2. Add/Edit:
   - Name: `VITE_API_URL`
   - Value: `https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net`
   - Environments: ✅ Production, ✅ Preview, ✅ Development
3. Save

### Bước 3: Redeploy Frontend

```bash
cd FrontEnd
npx vercel --prod
```

## 🎉 Done!

Test: https://vihis.vercel.app

---

**Chạy ngay:** `cd BackEnd && git push` để deploy backend!

