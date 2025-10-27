# ⚡ Làm ngay - 5 phút để fix lỗi

## 🎯 Mục tiêu
Fix lỗi `ERR_CONNECTION_REFUSED` bằng cách set backend URL cho Vercel frontend

## ✅ 3 bước đơn giản

### 1️⃣ Set Environment Variable

Mở link này:
**https://vercel.com/binzdapoet0209-7462s-projects/vihis/settings/environment-variables**

Click "Add New"

```
Name:  VITE_API_URL
Value: https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net
```

Chọn tất cả environments → **Save**

### 2️⃣ Redeploy

Vào: **https://vercel.com/binzdapoet0209-7462s-projects/vihis/deployments**

Click **...** → **Redeploy**

### 3️⃣ Test

Mở: **https://vihis.vercel.app**

Should work! ✅

---

## 🔍 Nếu vẫn lỗi

Check backend logs trong Azure Portal:
1. Azure Portal → App Service
2. Log stream
3. Should see: "✅ CORS Production enabled"

---

**Total time:** ~2 phút  
**Result:** App hoạt động bình thường!

