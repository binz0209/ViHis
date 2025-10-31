# AUTH_JWT – Phase 4: Debug (Result)

## Test execution summary
- Phạm vi: Chạy toàn bộ test + thu thập coverage
- Kết quả: 87 tổng, 84 pass, 3 fail (đều thuộc AI_QA Integration do API 429). Toàn bộ test AUTH pass.

## Quan sát cho AUTH
- Sai khác hành vi hiện tại (đã được test ghi nhận):
  - ChangePassword thiếu token → trả 404 (nếu tuân thủ chuẩn auth middleware thì nên 401). Test đánh dấu “current behavior”.
  - Login với username có khoảng trắng đầu/cuối → 401 (không tự trim). Ghi nhận là “current behavior”.
- Không thấy null-ref hay exception không được xử lý trong luồng AUTH.

## Artifacts
- Coverage report: `BackEnd/coveragereport-final/index.html`.
- Line coverage tổng: ~29.2%. Xem chi tiết theo file tại `VietHistory.Api_AuthController.html`, `VietHistory.Infrastructure_JwtService.html`.

## Đề xuất (tuỳ chọn)
- Chuẩn hoá phản hồi 401 cho trường hợp thiếu/invalid token ở `ChangePassword` để thống nhất.
- Thêm validation/normalization đầu vào (trim username, basic password policy) ở `AuthController`.
- Bổ sung test negative-path cho JSON sai kiểu khi thêm các attribute validation.
