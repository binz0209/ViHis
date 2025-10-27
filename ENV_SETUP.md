# ✅ ViHis đã deploy thành công!

## 🌐 URLs

- **Preview:** https://vihis-8pd47ko7p-binzdapoet0209-7462s-projects.vercel.app
- **Production:** https://vihis.vercel.app (cần add env var trước)

## ⚙️ Setup Environment Variables

### Option 1: Qua Vercel Dashboard (Recommended)

1. Mở https://vercel.com/binzdapoet0209-7462s-projects/vihis
2. Vào **Settings** → **Environment Variables**
3. Click **Add New**
4. Add variable:
   ```
   Name: VITE_API_URL
   Value: http://localhost:5000
   ```
   (Hoặc URL backend production nếu có)
5. Chọn environments: **Production**, **Preview**, **Development**
6. Click **Save**

### Option 2: Qua CLI

```bash
cd FrontEnd

# Add cho production
npx vercel env add VITE_API_URL production
# Nhập value: http://localhost:5000

# Add cho preview  
npx vercel env add VITE_API_URL preview

# Add cho development
npx vercel env add VITE_API_URL development
```

## 🚀 Deploy Production

```bash
cd FrontEnd
npx vercel --prod
```

URL production: https://vihis.vercel.app

## ⚠️ Lưu ý

Frontend đang cần backend để hoạt động:
- Backend hiện tại chạy ở `http://localhost:5000`
- Nếu chưa có backend production, bạn cần:
  1. Deploy backend lên Railway/Render/Azure
  2. Update `VITE_API_URL` = URL backend production

## ✅ Checklist

- [x] Frontend deploy thành công
- [ ] Add environment variable `VITE_API_URL`
- [ ] Deploy production version
- [ ] Test chat functionality
- [ ] Test login/logout

## 🧪 Test Deployment

1. Mở https://vihis-8pd47ko7p-binzdapoet0209-7462s-projects.vercel.app
2. Test chat với AI
3. Test tạo chat box mới
4. Test login (nếu backend ready)

---

**Hiện tại:** Frontend đã live nhưng cần backend để hoạt động đầy đủ.

