# 🔧 Set VITE_API_URL cho Vercel

## ❌ Vấn đề hiện tại

Frontend đang gọi `http://localhost:5000` thay vì Azure URL.

## ✅ Giải pháp - 2 cách

### Cách 1: Vercel Dashboard (Recommended)

1. **Mở:** https://vercel.com/binzdapoet0209-7462s-projects/vihis/settings/environment-variables

2. **Click "Add New"**

3. **Fill thông tin:**
   ```
   Name:  VITE_API_URL
   Value: https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net
   ```

4. **Select Environments:**
   - ✅ Production
   - ✅ Preview  
   - ✅ Development

5. **Click "Save"**

6. **Redeploy:**
   - Vào tab **Deployments**
   - Click **...** trên deployment mới nhất
   - Click **Redeploy**

### Cách 2: Vercel CLI

```bash
cd FrontEnd

# Add cho Production
npx vercel env add VITE_API_URL production
# Nhập: https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net

# Add cho Preview
npx vercel env add VITE_API_URL preview
# Nhập: https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net

# Add cho Development
npx vercel env add VITE_API_URL development
# Nhập: https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net

# Redeploy
npx vercel --prod
```

## ✅ Verify

Sau khi deploy xong:

1. Mở https://vihis.vercel.app
2. F12 → Console
3. Should see network calls tới Azure URL, không phải localhost
4. No more ERR_CONNECTION_REFUSED

## 🎯 Expected Result

**Before:**
```
GET http://localhost:5000/api/v1/chat/boxes
❌ ERR_CONNECTION_REFUSED
```

**After:**
```
GET https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net/api/v1/chat/boxes
✅ 200 OK
```

---

**Nhanh nhất:** Dùng Cách 1 (Dashboard) - chỉ 2 phút!

