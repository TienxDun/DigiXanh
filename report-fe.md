# Báo cáo Frontend - Tạo Component Đăng Ký (RegisterComponent)

## 1. Các công việc đã thực hiện

### 1.1. Tạo RegisterComponent
- Đã sử dụng Angular CLI để tạo component `RegisterComponent` dưới dạng standalone trong module `AuthModule` (`src/app/views/auth/register`).
- Đã thêm route `/auth/register` trong `auth-routing.module.ts` để người dùng có thể truy cập bằng đường dẫn này.

### 1.2. Cấu hình Form Đăng Ký bằng @ngx-formly
- Đã tích hợp các module của Formly (`FormlyModule`, `FormlyBootstrapModule`) vào `RegisterComponent`.
- Xây dựng fileds cho form thông qua `FormlyFieldConfig` với 4 trường:
  - `fullName` (Họ và tên): Loại input thường, bắt buộc.
  - `email` (Email): Loại email, bắt buộc, validation email chuẩn.
  - `password` (Mật khẩu): Loại password, bắt buộc, yêu cầu tối thiểu (`minLength`) là 6 ký tự.
  - `confirmPassword` (Xác nhận mật khẩu): Loại password, bắt buộc. Có tích hợp **custom validation** tên là `passwordMatch` để so sánh trực tiếp giá trị với trường `password`.

### 1.3. Tích hợp UI với CoreUI
- Đã áp dụng `CardModule`, `FormModule`, `ButtonModule`, `SpinnerModule`, `ToastModule` của `@coreui/angular` vào template HTML của ứng dụng để làm giao diện đẹp và chuẩn.
- Bố cục form được đặt ở giữa giao diện người dùng. Hiển thị nút "Đăng nhập" để người dùng điều hướng linh hoạt giữa Đăng ký và Đăng nhập.
- Trạng thái **Loading**: Trong khi gửi API, nút Đăng ký sẽ bị Disabled và hiển thị một chiếc spinner (vòng quay nhỏ) kế bên chữ "Tạo tài khoản".
- Thông báo qua **Toast**: Sử dụng trực tiếp `c-toaster` và `c-toast` để hiển thị popup thành công (màu xanh lá) và lỗi (màu đỏ) ở góc trên, bên phải thông qua thuộc tính `[visible]="showToast"`.

### 1.4. Xử lý API & Validation lỗi từ Server
- Đã cập nhật `app.config.ts`, thêm rule xử lý message `serverError` cho Formly.
- Cập nhật `AuthService`: Thêm hàm `register(data)` với `HttpClient` dùng để gọi API `POST /api/auth/register` lên Backend.
- Trong `RegisterComponent`, xử lý phản hồi từ `AuthService.register()`:
  - Nếu API lỗi xác thực (VD: Email trùng lặp), mã nguồn sẽ đọc `err.error.errors` của .NET và đẩy ngược từng lỗi vào các trường tương ứng của Formly bằng `field.formControl.setErrors({ serverError: err... })`. Sau đó mark field là "touched" để Formly hiển thị báo lỗi màu đỏ ngay dưới thẻ input của email hoặc username.
  - Hiển thị C-Toast (thông báo popup) thành công hoặc thất bại cho người dùng. Khi thành công, form sẽ đợi 1,5 giây để tự động navigate tới trang Đăng nhập (`/auth/login`).

---

## 2. Hướng dẫn Test Component

### Bước 1: Khởi động Backend + Frontend
Hãy chắc chắn rằng bạn đang chạy cả hai phía:
- Backend: Cổng `.NET` (ví dụ `http://localhost:5xxx`), nhớ kiểm tra lại biến môi trường `environment.ts` (`/api`). Hiện tại Frontend đã đang chạy `ng serve` hoặc `npm start`.

### Bước 2: Truy cập trang Đăng ký
Vào trên trình duyệt qua URL (phụ thuộc vào hệ thống route của bạn, mặc định chạy localhost:4200):
👉 `http://localhost:4200/#/auth/register` (Hoặc nếu routing không có `#`, có thể là `http://localhost:4200/auth/register`)

### Bước 3: Test Validation trực tiếp của Formly (Client-side)
- Bấm vào nút **"Tạo tài khoản"** khi chưa nhập gì ⇒ Các trường sẽ báo lỗi "Trường này là bắt buộc".
- Nhập email sai định dạng (vd: `abc`) ⇒ Báo lỗi "Email không hợp lệ".
- Nhập mật khẩu dưới 6 ký tự (vd: `12345`) ⇒ Báo lỗi "Mật khẩu phải có ít nhất 6 ký tự".
- Nhập `confirmPassword` không khớp với `password` ⇒ Báo lỗi "Mật khẩu xác nhận không khớp".

### Bước 4: Test Đăng ký thành công
- Điền toàn bộ thông tin hợp lệ (Email duy nhất, pass trùng khớp, pass từ 6 ký tự).
- Bấm "Tạo tài khoản". 
- Bạn sẽ thấy nút bị vô hiệu hóa kèm vòng quay spinner (để ngăn người dùng bấm spam nhiều lần).
- Pop-up Toast hiện xanh lá "Đăng ký thành công!", sau đó khoảng 1.5 giây hệ thống sẽ tự động chuyển hướng qua trang `/auth/login`.

### Bước 5: Test Đăng ký thất bại / Lỗi Backend
- Thực hiện đăng ký lại một tài khoản với **Email đã tồn tại** trong cơ sở dữ liệu.
- Bấm "Tạo tài khoản".
- Bạn sẽ nhận được thông báo Toast đỏ "Vui lòng kiểm tra lại thông tin!". Đồng thời Formly sẽ móc nối dữ liệu trả về từ ASP.NET Core và in lỗi màu đỏ (như *"Email này đã được sử dụng"*) ngay dưới trường Email.
