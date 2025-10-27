# 🚀 Setup ViHis với Azure Backend

## ✅ Backend URL
**Backend:** https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net

## 🔧 Bước 1: Deploy Backend lên Azure

### Option A: Qua Azure Portal

1. **Rebuild và Push code:**
```bash
cd BackEnd
git add .
git commit -m "Update CORS for Vercel"
git push
```

2. **Azure DevOps hoặc GitHub Actions sẽ auto deploy**

### Option B: Qua Visual Studio

1. Right-click project `VietHistory.Api`
2. Publish → Azure
3. Create new App Service hoặc chọn existing
4. Deploy

### Option C: Qua Azure CLI

```bash
cd BackEnd/VietHistory.Api

# Login Azure
az login

# Create resource group (if not exists)
az group create --name ViHisRG --location westus

# Create App Service Plan
az appservice plan create --name ViHisPlan --resource-group ViHisRG --sku FREE

# Create Web App
az webapp create --resource-group ViHisRG --plan ViHisPlan --name vihisprj

# Deploy
az webapp deployment source config --name vihisprj --resource-group ViHisRG --repo-url https://github.com/your-repo --branch main --manual-integration
```

## 🎯 Bước 2: Update Vercel Environment Variable

1. Mở https://vercel.com/binzdapoet0209-7462s-projects/vihis
2. **Settings** → **Environment Variables**
3. Update hoặc Add:
   ```
   Name: VITE_API_URL
   Value: https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net
   ```
4. Chọn: Production, Preview, Development
5. **Save**

## 🔄 Bước 3: Redeploy Frontend

```bash
cd FrontEnd
npx vercel --prod
```

Hoặc qua Dashboard:
- Vào **Deployments**
- Click **Redeploy** ở deployment mới nhất

## ✅ Test

1. Mở https://vihis.vercel.app
2. Test các features:
   - Chat với AI ✅
   - Tạo chat box ✅
   - Login/Logout ✅
   - Mobile responsive ✅

## 🐛 Troubleshooting

### CORS Error

Nếu gặp CORS error:
1. Check backend logs trên Azure
2. Verify CORS setting trong `Program.cs`
3. Backend phải set `Production` environment

### API Not Found

```bash
# Test backend
curl https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net/api/v1/ai/ask
```

### Environment Variable Not Working

1. Check variable name: `VITE_API_URL` (VITE prefix required)
2. Must rebuild frontend after adding env
3. Check Vercel deployment logs

## 📊 URLs

- **Frontend:** https://vihis.vercel.app
- **Backend:** https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net
- **Swagger:** https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net/swagger

## ⚙️ Environment Variables Summary

### Vercel (Frontend)
```
VITE_API_URL = https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net
```

### Azure (Backend)
```
ASPNETCORE_ENVIRONMENT = Production
Mongo__ConnectionString = <from appsettings>
Gemini__ApiKey = <from appsettings>
```

---

**Status:** ✅ Ready to deploy!

