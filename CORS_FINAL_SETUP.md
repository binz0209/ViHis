# 🔐 CORS & Deployment Setup - ViHis

## 📊 URLs

- **Frontend:** https://vihis.vercel.app
- **Backend:** https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net

## ✅ CORS Configuration

### Backend (Đã update)

**File:** `BackEnd/VietHistory.Api/Program.cs`

```csharp
// Production CORS policy allows:
✅ localhost (any port)
✅ vercel.app domains (any subdomain)
✅ Specific origins from config
```

**File:** `BackEnd/VietHistory.Api/appsettings.json`

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173", 
      "http://localhost:3001",
      "https://vihis.vercel.app"
    ]
  }
}
```

## 🚀 Deploy Steps

### Bước 1: Deploy Backend lên Azure

```bash
cd BackEnd

# Commit changes
git add .
git commit -m "Update CORS for production"

# Push to trigger Azure deployment
git push
```

**Hoặc qua Azure Portal:**
1. Azure Portal → App Service → Deployment Center
2. Sync repository
3. Redeploy

### Bước 2: Set Vercel Environment Variable

**Cách nhanh nhất:**

1. Mở: https://vercel.com/binzdapoet0209-7462s-projects/vihis/settings/environment-variables
2. Click **Add New** hoặc **Edit** existing
3. Set:
   ```
   Name:  VITE_API_URL
   Value: https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net
   ```
4. Environments: ✅ Production, ✅ Preview, ✅ Development
5. **Save**

### Bước 3: Redeploy Frontend

**Option A - Dashboard:**
1. https://vercel.com/binzdapoet0209-7462s-projects/vihis/deployments
2. Click **...** trên deployment mới nhất
3. **Redeploy**

**Option B - CLI:**
```bash
cd FrontEnd
npx vercel --prod
```

## ✅ Verify Setup

### Test 1: Backend CORS

```bash
curl -H "Origin: https://vihis.vercel.app" \
     -H "Access-Control-Request-Method: POST" \
     -X OPTIONS \
     https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net/api/v1/ai/ask \
     -v
```

**Expected:**
```
Access-Control-Allow-Origin: https://vihis.vercel.app
```

### Test 2: Frontend

1. Mở https://vihis.vercel.app
2. F12 → Console
3. Should see: `✅ CORS Production enabled for: ...`
4. No CORS errors ✅

### Test 3: Chat với AI

1. Type: "Ai là Trần Hưng Đạo?"
2. Click Send
3. Should get response ✅

## 🎯 Current Status

- ✅ Backend CORS config updated
- ✅ Backend URL: Azure
- ✅ Frontend URL: Vercel
- ⏳ Need: Deploy backend with new CORS
- ⏳ Need: Set VITE_API_URL in Vercel
- ⏳ Need: Redeploy frontend

## 📋 Quick Checklist

- [ ] Git push backend changes
- [ ] Wait for Azure deployment
- [ ] Add VITE_API_URL to Vercel
- [ ] Redeploy frontend
- [ ] Test https://vihis.vercel.app
- [ ] Test chat functionality
- [ ] Test login/logout
- [ ] Test mobile responsive

## 🔧 If CORS Still Fails

### Check Backend Logs

Azure Portal → App Service → Log stream

Look for:
```
✅ CORS Production enabled for: ...
```

### Manual CORS Test

```bash
# Test if backend allows Vercel
curl -H "Origin: https://vihis.vercel.app" \
     -H "Access-Control-Request-Method: POST" \
     -H "Access-Control-Request-Headers: content-type" \
     -X OPTIONS \
     https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net/api/v1/chat/boxes
```

Should return CORS headers.

### Update Azure Configuration

If needed, check Azure App Service Settings:
- **CORS → Allowed Origins:** Add `https://vihis.vercel.app`
- Or delete CORS setting, rely on code configuration

---

**Sau khi hoàn thành:** App sẽ live và functional tại https://vihis.vercel.app

