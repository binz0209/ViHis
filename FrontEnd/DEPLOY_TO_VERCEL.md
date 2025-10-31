# Hướng dẫn Deploy Frontend lên Vercel

## Vấn đề
CLI đang có vấn đề với cấu hình project. Cách tốt nhất là deploy qua Vercel Dashboard.

## Các bước

### 1. Truy cập Vercel Dashboard
Mở trình duyệt và vào: https://vercel.com/binzdapoet0209-7462s-projects/vihis

### 2. Click vào "Deployments" tab

### 3. Click vào nút "Deploy" hoặc "Redeploy"

### 4. Hoặc sử dụng CLI với import
1. Xóa folder `.vercel` trong FrontEnd:
```bash
cd FrontEnd
Remove-Item -Recurse -Force .vercel
```

2. Deploy lại:
```bash
cd FrontEnd
vercel --prod
```

### 5. Cách khác: Import từ Git (Khuyến nghị)
1. Vào Vercel Dashboard
2. Chọn project "vihis"
3. Vào Settings > Git
4. Disconnect repo hiện tại (nếu có)
5. Click "Connect Git Repository"
6. Chọn repo: https://github.com/binzdapoet0209-7462s/ViHis.git
7. Set:
   - Root Directory: `FrontEnd`
   - Build Command: `npm run build`
   - Output Directory: `dist`
8. Click "Deploy"

## URLs
- Production: https://vihis.vercel.app
- Project URL: https://vercel.com/binzdapoet0209-7462s-projects/vihis


