# 🔧 Fix: ERR_CONNECTION_REFUSED

## ❌ Lỗi hiện tại
```
GET http://localhost:5000/api/v1/chat/boxes 
ERR_CONNECTION_REFUSED
```

**Nguyên nhân:** Frontend đang gọi localhost thay vì Azure backend

## ✅ Fix trong 5 phút

### Step 1: Open Vercel Dashboard
https://vercel.com/binzdapoet0209-7462s-projects/vihis/settings/environment-variables

### Step 2: Add Environment Variable

Click **"Add New"** button

Fill form:
```
╔═══════════════════════════════════════╗
║ Environment Variable                  ║
╠═══════════════════════════════════════╣
║ Key:   VITE_API_URL                   ║
║ Value: https://vihisprj-g2gyaeh...   ║
║        .azurewebsites.net             ║
╚═══════════════════════════════════════╝
```

Scroll down → Select:
```
✅ Production
✅ Preview  
✅ Development
```

Click **Save**

### Step 3: Redeploy

1. Go to: https://vercel.com/binzdapoet0209-7462s-projects/vihis/deployments
2. Find latest deployment
3. Click **...** (three dots)
4. Click **Redeploy**
5. Wait ~30 seconds

### Step 4: Test

1. Open: https://vihis.vercel.app
2. Should work now! ✅

---

## 🎯 Quick Commands

Nếu thích CLI thay vì Dashboard:

```bash
# Set env variable
cd FrontEnd
npx vercel env add VITE_API_URL production
# Paste: https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net

npx vercel env add VITE_API_URL preview
# Same URL

npx vercel env add VITE_API_URL development  
# Same URL

# Deploy
npx vercel --prod
```

---

**Sau khi fix:** App sẽ connect tới Azure backend thành công! 🎉

