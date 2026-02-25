# BE Report - 2026-02-25

## 1) Tổng quan đã hoàn thành
Đã triển khai chức năng đăng ký tài khoản backend cho DigiXanh API theo yêu cầu:

- Tạo DTO cho request/response đăng ký.
- Tạo endpoint `POST /api/auth/register`.
- Dùng `UserManager<ApplicationUser>` để tạo user với:
  - `UserName = Email`
  - `Email = Email`
  - `FullName = FullName`
- Gán role mặc định `User` cho tài khoản mới.
- Seed role `User` nếu chưa tồn tại.
- Trả lỗi chi tiết `400 BadRequest` theo field.
- Cập nhật Swagger để FE test thuận tiện.
- Viết unit test cho controller bằng mock.

## 2) File đã tạo / chỉnh sửa

### API
- `DigiXanh.API/Controllers/AuthController.cs`
- `DigiXanh.API/DTOs/Auth/RegisterRequest.cs`
- `DigiXanh.API/DTOs/Auth/RegisterResponse.cs`
- `DigiXanh.API/DTOs/Common/ValidationErrorResponse.cs`
- `DigiXanh.API/Constants/DefaultRoles.cs`
- `DigiXanh.API/Program.cs`

### Test
- `DigiXanh.API.Tests/DigiXanh.API.Tests.csproj`
- `DigiXanh.API.Tests/Controllers/AuthControllerTests.cs`
- Đã xóa file mẫu mặc định:
  - `DigiXanh.API.Tests/UnitTest1.cs`

### Solution
- `DigiXanh.sln` (đã add test project)

## 3) Chi tiết endpoint đăng ký

### URL
`POST /api/auth/register`

### Request body
```json
{
  "fullName": "Nguyen Van A",
  "email": "user@example.com",
  "password": "StrongPassword123!"
}
```

### Success response (200)
```json
{
  "id": "<user-id>",
  "email": "user@example.com",
  "fullName": "Nguyen Van A"
}
```

### Error response (400) - email đã tồn tại
```json
{
  "errors": {
    "email": [
      "Email đã được sử dụng"
    ]
  }
}
```

### Error response (400) - password yếu (từ Identity)
```json
{
  "errors": {
    "password": [
      "Passwords must be at least 6 characters."
    ]
  }
}
```

## 4) Logic xử lý lỗi đã áp dụng

- Kiểm tra email tồn tại trước bằng `FindByEmailAsync`.
- Nếu tồn tại: trả `email = "Email đã được sử dụng"`.
- Nếu `CreateAsync` fail: map lỗi Identity thành object `errors` theo field:
  - `password` nếu lỗi liên quan mật khẩu
  - `email` nếu lỗi liên quan email
  - `general` cho lỗi khác
- Nếu tạo role hoặc gán role fail sau khi tạo user:
  - rollback mềm bằng xóa user vừa tạo
  - trả lỗi chi tiết tương ứng

## 5) Cập nhật Swagger

Swagger đã hiển thị:
- Request schema: `RegisterRequest`
- Response `200`: `RegisterResponse`
- Response `400`: `ValidationErrorResponse`

Thuộc tính này được khai báo trực tiếp trong `AuthController` với `ProducesResponseType`.

## 6) Cách test

### A. Chạy API
Tại thư mục root:

```bash
dotnet run --project DigiXanh.API
```

Mở Swagger:
- `https://localhost:<port>/swagger`
hoặc
- `http://localhost:<port>/swagger`

### B. Test thủ công endpoint register trên Swagger
1. Vào `POST /api/auth/register`
2. Nhập JSON mẫu
3. Bấm Execute
4. Kiểm tra:
   - 200 nếu tạo user thành công
   - 400 nếu email trùng hoặc password không đạt policy

### C. Chạy unit test
Tại thư mục root:

```bash
dotnet test DigiXanh.sln
```

Kết quả hiện tại:
- Total: 3
- Passed: 3
- Failed: 0

## 7) Lưu ý vận hành

- Nếu đang chạy API và test bị lỗi lock file `DigiXanh.API.exe`, dừng process API trước rồi chạy lại test.
- Role `User` được đảm bảo tồn tại khi app startup (seed trong `Program.cs`).

## 8) Đề xuất bước tiếp theo

- Triển khai `POST /api/auth/login` trả JWT cho FE.
- Thêm refresh token (nếu scope sprint cho phép).
- Thêm integration test cho auth flow end-to-end.
