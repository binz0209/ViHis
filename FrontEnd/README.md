# VietHistory Frontend

Frontend cho hệ thống Tra cứu Lịch sử Việt Nam sử dụng React + TypeScript + Vite.

## Tính năng

- 💬 Chatbox để đặt câu hỏi về lịch sử Việt Nam
- 🎨 UI hiện đại, responsive
- ⚡ Fast reload với Vite
- 🔗 Tích hợp với backend API

## Cài đặt

```bash
cd FrontEnd
npm install
```

## Chạy ứng dụng

```bash
npm run dev
```

Ứng dụng sẽ chạy tại `http://localhost:3000`

## Cấu hình

Mặc định, frontend sẽ kết nối với backend tại `http://localhost:5000`. 

Để thay đổi URL API, tạo file `.env` trong thư mục `FrontEnd`:

```
VITE_API_URL=http://your-api-url:port
```

## Build cho production

```bash
npm run build
```

File build sẽ được tạo trong thư mục `dist/`.


