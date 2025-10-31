# Hướng dẫn Deploy Frontend lên Vercel

## Project hiện tại: vihis
URL: https://vercel.com/binzdapoet0209-7462s-projects/vihis

## Vấn đề hiện tại:
Vercel không tìm thấy output directory `dist` sau khi build

## Cách deploy qua Vercel Dashboard:

1. **Mở Vercel Dashboard:**
   - Truy cập: https://vercel.com/binzdapoet0209-7462s-projects/vihis

2. **Cập nhật Project Settings:**
   - Vào Settings → General
   - Framework Preset: Vite
   - Build Command: `npm run build`
   - Output Directory: `dist`
   - Install Command: `npm install`
   - Root Directory: (để trống)

3. **Deploy:**
   - Click "Deployments"
   - Click "Redeploy" trên deployment mới nhất
   - Hoặc push code lên Git và auto-deploy

## Hoặc qua Git:

1. **Push code lên Git repository**
2. **Connect Git trong Vercel:**
   - Vào Settings → Git
   - Connect repository
   - Vercel sẽ auto-deploy khi có commit mới

## Environment Variables cần thêm:

Trong Vercel Dashboard → Settings → Environment Variables, thêm:
- `VITE_API_URL`: Backend API URL (vd: http://localhost:5000 hoặc https://your-backend-url)

## Build Output:
- Thư mục dist đã được build thành công tại local
- Cần Vercel build từ source code (không từ thư mục dist có sẵn)

