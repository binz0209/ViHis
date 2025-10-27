# Cấu hình CORS cho VietHistory API

## 📋 Tổng quan

CORS (Cross-Origin Resource Sharing) đã được cấu hình để cho phép frontend kết nối với backend API.

## ⚙️ Cấu hình

### File: `Program.cs`

CORS được cấu hình với 3 policies:

1. **Development** - Cho môi trường development
   - Cho phép origins cụ thể từ config
   - Cho phép credentials (cookies, headers)
   - Cho phép tất cả headers và methods

2. **Production** - Cho môi trường production
   - Chỉ cho phép origins trong config
   - Cho phép credentials
   - Secure hơn development

3. **AllowAll** - Fallback cho testing
   - Cho phép tất cả origins
   - Sử dụng khi cần test integration

### File: `appsettings.json`

Các origins được phép:
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",  // Vite dev server
      "http://localhost:5173",  // Vite alternate port
      "http://localhost:3001"   // Backup port
    ]
  }
}
```

## 🎯 Default Origins

Mặc định backend cho phép các origins sau:
- `http://localhost:3000` - Frontend chính
- `http://localhost:5173` - Vite default port
- `http://localhost:3001` - Backup port

## 🔧 Cách thêm origin mới

### Option 1: Qua appsettings.json

Sửa file `BackEnd/VietHistory.Api/appsettings.json`:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://your-custom-domain.com"
    ]
  }
}
```

### Option 2: Qua Environment Variables

```bash
export Cors__AllowedOrigins__0="http://localhost:3000"
export Cors__AllowedOrigins__1="http://your-domain.com"
```

### Option 3: Qua Code (không khuyến nghị)

Sửa file `Program.cs`:
```csharp
var allowedOrigins = new[] { 
    "http://localhost:3000",
    "http://your-domain.com" 
};
```

## 🚀 Sử dụng

### Development
Backend tự động sử dụng policy "Development" khi `ASPNETCORE_ENVIRONMENT=Development`

### Production
Set `ASPNETCORE_ENVIRONMENT=Production` và backend sẽ dùng policy "Production"

## ✅ Kiểm tra CORS

### 1. Kiểm tra qua Console
Khi start backend, bạn sẽ thấy:
```
✅ CORS Development enabled for: http://localhost:3000, http://localhost:5173, http://localhost:3001
```

### 2. Test qua Browser Console

Mở browser console tại frontend và chạy:
```javascript
fetch('http://localhost:5000/api/v1/ai/ask', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    question: 'test',
    language: 'vi'
  })
})
  .then(res => res.json())
  .then(data => console.log('Success:', data))
  .catch(err => console.error('CORS Error:', err))
```

### 3. Test với cURL

```bash
curl -X POST http://localhost:5000/api/v1/ai/ask \
  -H "Content-Type: application/json" \
  -H "Origin: http://localhost:3000" \
  -d '{"question":"test","language":"vi"}' \
  -v
```

## 🐛 Troubleshooting

### Lỗi "Access-Control-Allow-Origin"

**Triệu chứng:**
```
Access to fetch at 'http://localhost:5000/api/v1/ai/ask' from origin 'http://localhost:3000' 
has been blocked by CORS policy
```

**Giải pháp:**
1. Kiểm tra origin có trong `appsettings.json`
2. Restart backend server
3. Kiểm tra console log để xem CORS policy được enable

### Lỗi "AllowAnyOrigin" không hoạt động với credentials

**Vấn đề:**
Không thể dùng `AllowCredentials()` cùng với `AllowAnyOrigin()`

**Giải pháp:**
Đã fix bằng cách dùng `WithOrigins()` thay vì `AllowAnyOrigin()` trong development/production policies.

### Frontend không gọi được API

**Kiểm tra:**
1. Backend đang chạy tại đúng port (5000)
2. Frontend URL trong list AllowedOrigins
3. CORS middleware được gọi trước MapControllers (đã fix)

## 📚 Tài liệu tham khảo

- [ASP.NET Core CORS Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/cors)
- [MDN - CORS](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS)


