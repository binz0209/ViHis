# 🚀 Deploy ViHis lên Vercel - Hướng dẫn

## Tình huống hiện tại
- ✅ Frontend code sẵn sàng
- ✅ Build local thành công (dist folder)
- ❌ Vercel deploy chưa thành công vì cấu hình build

## 🔧 Giải pháp: Deploy qua Vercel Dashboard

### Cách 1: Deploy qua Dashboard (Recommended)

1. **Mở https://vercel.com**
   - Login vào tài khoản của bạn

2. **New Project hoặc Update Project Settings**
   - Vào project `vihis` đã tạo trước đó
   - Vào **Settings** → **General**

3. **Cấu hình Build Settings**
   ```
   Framework Preset: Other
   Root Directory: (để trống hoặc .)
   Build Command: npm run build
   Output Directory: dist
   Install Command: npm install
   ```

4. **Save Settings**

5. **Redeploy**
   - Vào tab **Deployments**
   - Click **Redeploy** ở deployment gần nhất

### Cách 2: Xóa Project và Tạo lại

```bash
# Xóa project cũ trên Vercel dashboard
# Sau đó chạy lại:

cd FrontEnd
npx vercel
```

Khi được hỏi:
- **Setup and deploy?** → Yes
- **Which scope?** → Your account
- **Link to existing project?** → No
- **What's your project's name?** → vihis
- **In which directory is your code located?** → ./
- **Override settings?** → Yes
- **Development Command:** `npm run dev`
- **Build Command:** `npm run build`
- **Output Directory:** `dist`
- **Install Command:** `npm install`

### Cách 3: Fix trong Dashboard

1. Vào **Settings** → **General** của project
2. Sửa các fields:
   - **Build Command:** `npm run build`
   - **Output Directory:** `dist`
   - **Install Command:** `npm install`
3. **Development Command:** `npm run dev`
4. Click **Save**

5. **Redeploy** từ tab Deployments

## ⚙️ Add Environment Variable

Sau khi deploy thành công:

1. Vào **Settings** → **Environment Variables**
2. Add variable:
   ```
   Name: VITE_API_URL
   Value: http://localhost:5000
   ```
   (hoặc URL backend production của bạn)

3. **Redeploy** để apply environment variables

## ✅ Verify Deployment

Sau khi deploy thành công:
- Frontend URL: https://vihis-your-username.vercel.app
- Test chat box hoạt động
- Test login/logout

## 🐛 Nếu vẫn lỗi

```bash
# Check project settings:
cd FrontEnd
cat package.json  # Verify scripts
npm run build     # Test build locally
ls dist/          # Verify output folder
```

## 📞 Checklist

- [ ] Vercel dashboard cấu hình đúng
- [ ] Build command: `npm run build`
- [ ] Output directory: `dist`
- [ ] Install command: `npm install`
- [ ] Environment variable `VITE_API_URL` được thêm
- [ ] Redeploy thành công
- [ ] Frontend accessible trên Vercel URL

---

**Bây giờ hãy vào Vercel Dashboard và update settings theo hướng dẫn trên.**

