# 🚀 Final Setup - ViHis Production

## 🎯 URLs
- **Frontend:** https://vihis.vercel.app  
- **Backend:** https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net

## ❌ Vấn đề hiện tại
Frontend đang gọi `localhost:5000` → Cần set environment variable

## ✅ Giải pháp - 3 bước

### Bước 1: Set Environment Variable trong Vercel

**Mở link này:**
https://vercel.com/binzdapoet0209-7462s-projects/vihis/settings/environment-variables

**Hoặc:**
1. Vào https://vercel.com
2. Chọn project `vihis`
3. Settings → Environment Variables

**Thêm variable:**
- **Key:** `VITE_API_URL`
- **Value:** `https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net`
- **Environments:** Production, Preview, Development
- **Save**

### Bước 2: Redeploy Frontend

**Option A - Dashboard:**
1. https://vercel.com/binzdapoet0209-7462s-projects/vihis/deployments
2. Click **...** trên deployment mới nhất
3. **Redeploy**

**Option B - CLI:**
```bash
cd FrontEnd
npx vercel --prod
```

### Bước 3: Verify

1. Mở https://vihis.vercel.app
2. F12 → Network tab
3. Should see requests tới Azure URL
4. No more `localhost:5000` errors ✅

## 🎉 Done!

Sau khi redeploy:
- ✅ Frontend → Azure backend
- ✅ No CORS errors
- ✅ Chat works
- ✅ Mobile responsive

---

**Lưu ý:** Environment variables **chỉ** được inject vào bundle khi build. Vì vậy phải redeploy sau khi set env!

