# AUTH_JWT – Phase 5: Optimize (Result)

## Mục tiêu
- Tăng độ tin cậy, bảo mật, và coverage cho module AUTH.

## Tối ưu kỹ thuật đề xuất
1) Validation & Normalization
- Thêm `[Required]`, `[EmailAddress]`, `[MinLength]` cho `RegisterRequest`, `LoginRequest`, `ChangePasswordRequest`.
- Trim `Username`, chuẩn hoá lowercase nếu policy yêu cầu.
- Trả `400` sớm khi payload không hợp lệ (ModelState).

2) Chuẩn hoá HTTP Status
- `ChangePassword` khi thiếu/invalid token → `401` thay vì `404`.
- Thống nhất thông báo lỗi dạng `{ error: "..." }`.

3) Bảo mật JWT
- Đưa `JwtOptions` vào cấu hình chuẩn (UserSecrets/ENV), rotate key theo môi trường.
- Thêm claim `roles` nếu cần RBAC; kiểm tra audience/issuer chặt chẽ.

4) Logging & Auditing
- Log đăng nhập thành công/thất bại (ẩn password), đổi mật khẩu, đăng ký.
- Ghi `AuditLog` tối thiểu: userId, action, time, ip.

5) Test & Coverage
- Viết thêm test cho: model validation (400), token thiếu Bearer prefix (401), token sai định dạng (401).
- Rerun coverage cho riêng AUTH; mục tiêu ≥ 85% cho `AuthController` và `JwtService`.

## Lộ trình triển khai ngắn
- B1: Thêm attribute validation + normalize input.
- B2: Cập nhật `ChangePassword` trả 401 đúng chuẩn.
- B3: Bổ sung test tương ứng; cập nhật report.
- B4: Cấu hình JwtOptions qua ENV; kiểm thử smoke.
