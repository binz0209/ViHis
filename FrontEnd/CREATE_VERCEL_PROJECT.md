# Tạo project Vercel mới: vihisfpt

## Cách 1: Qua Vercel Dashboard

1. Mở https://vercel.com/new
2. Chọn "Import Git Repository"
3. Chọn repository: binz0209/ViHis
4. Configure Project:
   - **Name:** `vihisfpt`
   - **Root Directory:** `FrontEnd`
   - **Framework Preset:** Vite
   - **Build Command:** `npm run build`
   - **Output Directory:** `dist`
   - **Install Command:** `npm install`
5. Click "Deploy"

## Cách 2: Qua CLI với tên mới

```bash
cd FrontEnd
vercel add
```
Khi được hỏi:
- Project name: `vihisfpt`
- Confirm: Yes

Sau đó:
```bash
vercel --prod --yes
```

## Environment Variables cần thêm:

Trong Vercel Dashboard → Settings → Environment Variables:
```
VITE_API_URL=https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net
```

## Sau khi deploy:
- URL sẽ là: https://vihisfpt.vercel.app
- Hoặc custom domain nếu đã config

