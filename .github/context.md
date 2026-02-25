Dưới đây là file **context.md** hoàn chỉnh, tích hợp tất cả các quyết định và thông tin chi tiết cho dự án DigiXanh. File này sẽ là tài liệu tham khảo duy nhất cho cả hai Agent (FE và BE) xuyên suốt dự án.

---

# 🌱 DigiXanh – Project Context (Dành cho AI Agents)

## 1. Tổng quan dự án
**DigiXanh** là website thương mại điện tử bán cây xanh, được xây dựng với mục đích học hỏi công nghệ và quy trình Scrum.

- **Product Owner (PO):** Bạn (người dùng) – chịu trách nhiệm định hướng, ưu tiên backlog, review kết quả.
- **Development Team:** Hai AI agents – một phụ trách **Frontend (Angular)**, một phụ trách **Backend (ASP.NET Core)**.
- **Phương pháp:** Scrum (Sprints 1–2 tuần).
- **Mục tiêu cuối cùng:** Hoàn thành phiên bản MVP với các chức năng CRUD của admin, flow mua hàng của khách, áp dụng 3 mẫu thiết kế (Adapter, Decorator, Facade) và triển khai miễn phí.

## 2. Công nghệ sử dụng

### Backend
- **Framework:** ASP.NET Core 8 Web API
- **Database:** SQL Server + Entity Framework Core (Code First)
- **Authentication:** ASP.NET Core Identity (JWT)
- **Tích hợp:** Trefle API (lấy dữ liệu cây), VNPay Sandbox
- **Patterns:** Adapter, Decorator, Facade
- **Hosting:** Render (free plan)

### Frontend
- **Framework:** Angular (v17+)
- **UI Library:** CoreUI for Angular (dựa trên Bootstrap 5) – [https://coreui.io/angular/](https://coreui.io/angular/)
- **Form Handling:** @ngx-formly/core + @ngx-formly/bootstrap
- **Icons:** Font Awesome (free) – @fortawesome/fontawesome-free
- **State Management:** Service + BehaviorSubject (hoặc signals)
- **Hosting:** GitHub Pages (free)

### Công cụ & Môi trường
- **Version control:** Git (GitHub)
- **Project management:** GitHub Projects hoặc Trello
- **API testing:** Postman / Swagger
- **Deployment:** GitHub Actions (FE), Render (BE)

## 3. Kiến trúc tổng thể

### Backend Layers
```
Controllers → Services → Repositories (optional) → DbContext
             ↗ Patterns (Adapter, Decorator, Facade)
```
- **Controllers:** Xử lý request/response, gọi Service.
- **Services:** Chứa logic nghiệp vụ chính.
- **Repositories:** (Tùy chọn) Tách truy vấn DB, có thể dùng trực tiếp DbContext.
- **DTOs:** Tách biệt Entity và DTO (dùng AutoMapper nếu muốn).
- **Pattern Implementations:** Đặt trong thư mục `Patterns/`.

### Frontend Modules
```
src/app/
├── core/               # Singleton services, guards, interceptors
├── shared/             # Shared components, directives, pipes
├── features/           # Feature modules (lazy-loaded)
│   ├── auth/           # Login, Register
│   ├── public/         # Home, plant list, plant detail (public)
│   ├── cart/           # Cart, checkout
│   └── admin/          # Plant management, dashboard (protected)
```
- Sử dụng **lazy-loading** cho Admin và Cart modules.
- **Auth guard** bảo vệ route Admin (kiểm tra role).

## 4. Product Backlog (User Stories ưu tiên MVP)

| ID    | User Story                                                                 | Ghi chú |
|-------|-----------------------------------------------------------------------------|---------|
| US01  | Đăng ký tài khoản                                                           |         |
| US02  | Đăng nhập                                                                   |         |
| US03  | Phân quyền Admin/User                                                        |         |
| US04  | Xem danh sách cây (Admin)                                                   |         |
| US05  | Thêm cây mới từ Trefle API                                                   |         |
| US06  | Sửa / Xoá cây (soft delete)                                                 |         |
| US07  | Dashboard thống kê đơn giản (Admin)                                         |         |
| US08  | Xem danh sách cây (Public)                                                  |         |
| US09  | Xem chi tiết cây + Thêm vào giỏ hàng                                        |         |
| US10  | Quản lý giỏ hàng (xem, sửa số lượng, xoá)                                   |         |
| US11  | Đặt hàng (nhập thông tin giao hàng, chọn thanh toán)                        |         |
| US12  | Thanh toán tiền mặt (Cash)                                                  | Adapter |
| US13  | Thanh toán VNPay (Sandbox)                                                  | Adapter |
| US14  | Áp dụng giảm giá theo số lượng (mua 2 giảm 5%, ≥3 giảm 7%)                  | Decorator |
| US15  | Xử lý đơn hàng (validate, tính giá, tạo order, thanh toán, email, xoá giỏ) | Facade |
| US16  | Thiết kế database (Migration)                                               |         |
| US17  | Triển khai API lên Render                                                    |         |
| US18  | Triển khai FE lên GitHub Pages                                               |         |

*Lưu ý:* Các story có thể được chia nhỏ thêm trong quá trình Sprint Planning.

## 5. Database Schema (MVP)

### Tables

**Users** (kế thừa IdentityUser)
- Bổ sung: FullName, Address, PhoneNumber (nếu cần)

**Categories**
- Id (int, PK)
- Name (nvarchar(100))

**Plants**
- Id (int, PK)
- Name (nvarchar(200))
- ScientificName (nvarchar(200))
- Description (nvarchar(max))
- Price (decimal(18,2))
- CategoryId (int, FK → Categories.Id)
- ImageUrl (nvarchar(500))
- IsDeleted (bit) – soft delete
- CreatedAt (datetime)
- UpdatedAt (datetime)

**Orders**
- Id (int, PK)
- UserId (string, FK → Users.Id)
- OrderDate (datetime)
- TotalAmount (decimal(18,2))
- Status (int) – enum: Pending, Paid, Shipped, Delivered, Cancelled
- ShippingAddress (nvarchar(500))
- Phone (nvarchar(20))
- PaymentMethod (int) – enum: Cash, VNPay
- TransactionId (nvarchar(100)) – dùng cho VNPay

**OrderItems**
- Id (int, PK)
- OrderId (int, FK → Orders.Id)
- PlantId (int, FK → Plants.Id)
- Quantity (int)
- UnitPrice (decimal(18,2)) – giá tại thời điểm mua

**Carts** (có thể lưu trong DB hoặc session, nhưng nên dùng DB để đồng bộ)
- Id (int, PK)
- UserId (string, FK → Users.Id)
- PlantId (int, FK → Plants.Id)
- Quantity (int)
- CreatedAt (datetime)

## 6. Chi tiết Design Patterns

### 6.1. Adapter Pattern – Thanh toán
**Interface:**
```csharp
public interface IPaymentAdapter
{
    Task<PaymentResult> ProcessPaymentAsync(Order order, PaymentInfo paymentInfo);
}
```
**Implementations:**
- `CashPaymentAdapter`: Chỉ cập nhật trạng thái Order thành Pending (hoặc Paid nếu giao hàng mới thu tiền).
- `VNPayPaymentAdapter`: Tạo URL thanh toán, gửi redirect URL về FE. Xử lý callback từ VNPay (IPN hoặc return URL) để cập nhật trạng thái.

**Sử dụng:** Trong `OrderProcessingFacade`, tùy theo `PaymentMethod` mà inject adapter tương ứng (có thể dùng factory hoặc DI với keyed services).

### 6.2. Decorator Pattern – Tính giá giảm dần
**Interface:**
```csharp
public interface IPriceCalculator
{
    decimal CalculatePrice(IEnumerable<CartItem> items);
}
```
**Implementations:**
- `BasePriceCalculator`: Tính tổng = Σ(Quantity * UnitPrice).
- `QuantityDiscountDecorator`: Nhận inner calculator và áp dụng % giảm nếu tổng số lượng đạt ngưỡng. Constructor: `QuantityDiscountDecorator(IPriceCalculator inner, int threshold, decimal discountPercent)`.

**Sử dụng:**
```csharp
var calculator = new BasePriceCalculator();
calculator = new QuantityDiscountDecorator(calculator, 2, 5);   // giảm 5% nếu >=2
calculator = new QuantityDiscountDecorator(calculator, 3, 7);   // giảm 7% nếu >=3 (sau khi giảm 5%? Cần định nghĩa rõ: ưu tiên mức cao nhất hoặc cộng dồn? Ở đây ta chọn áp dụng mức cao nhất nếu đủ điều kiện. Có thể decorator kiểm tra nếu số lượng >=3 thì áp dụng 7% thay vì 5%. Cần logic rõ ràng.)
```
**Logic:** Nếu tổng số lượng ≥3 → giảm 7%; nếu =2 → giảm 5%; khác → không giảm. Decorator nên kiểm tra điều kiện và trả về giá sau giảm.

### 6.3. Facade Pattern – Xử lý đơn hàng
**Lớp Facade:**
```csharp
public class OrderProcessingFacade
{
    private readonly IPriceCalculator _priceCalculator;
    private readonly IPaymentAdapter _paymentAdapter;
    private readonly IOrderRepository _orderRepo;
    private readonly IEmailService _emailService;
    private readonly ICartService _cartService;
    private readonly ApplicationDbContext _dbContext;

    public async Task<OrderResult> PlaceOrderAsync(Cart cart, ShippingInfo info, PaymentMethod method)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // 1. Validate (kiểm tra tồn kho nếu có, nhưng MVP bỏ qua)
            // 2. Tính tổng tiền dùng _priceCalculator
            var total = _priceCalculator.CalculatePrice(cart.Items);
            // 3. Tạo Order entity
            var order = new Order { UserId = cart.UserId, ... };
            // 4. Xử lý thanh toán qua adapter
            var paymentResult = await _paymentAdapter.ProcessPaymentAsync(order, ...);
            if (!paymentResult.Success) throw new Exception("Payment failed");
            // 5. Lưu order
            await _orderRepo.AddAsync(order);
            // 6. Gửi email xác nhận (bỏ qua nếu chưa có email service)
            // 7. Xoá giỏ hàng
            await _cartService.ClearCartAsync(cart.UserId);
            // 8. Commit transaction
            await transaction.CommitAsync();
            return new OrderResult { Success = true, OrderId = order.Id };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

## 7. Tích hợp Trefle API
- **Tài liệu:** [https://docs.trefle.io/](https://docs.trefle.io/)
- **API Key:** Cần đăng ký tài khoản Trefle để lấy key.
- **Endpoints chính:**
  - `GET /api/v1/plants/search?q={query}` – tìm kiếm cây theo tên
  - `GET /api/v1/plants/{id}` – lấy chi tiết cây
- **Cách dùng:** Admin nhập tên cây → gọi `search` → hiển thị danh sách → chọn → gọi `details` → tự động điền form thêm cây. Chỉ lưu vào DB sau khi admin nhập giá và danh mục.
- **Xử lý rate limit:** Trefle giới hạn số request, nên cache kết quả tìm kiếm trong phiên làm việc (session) hoặc dùng memory cache ngắn hạn.

## 8. Style Guide cho Frontend

### 8.1. Màu sắc (Brand Colors)
```scss
$primary: #2E7D32;   // Xanh lá đậm
$secondary: #81C784;  // Xanh lá nhạt
$success: #4CAF50;
$info: #29b6f6;
$warning: #ff9800;
$danger: #f44336;
$light: #f8f9fa;
$dark: #343a40;
```

### 8.2. Typography
- **Font chính:** `'Roboto', sans-serif`
- **Cỡ chữ:** 1rem = 16px (có thể điều chỉnh)

### 8.3. Component Customization (ví dụ)
- **Button:** border-radius 20px, padding 0.5rem 1.5rem.
- **Product Card:** border-radius 12px, box-shadow khi hover, image cover.
- **Container:** max-width 1400px, margin auto, padding 0 20px.

### 8.4. BEM Convention cho component tự viết
```html
<div class="product-card product-card--featured">
  <img class="product-card__image" src="...">
  <h3 class="product-card__title">Cây XYZ</h3>
  <span class="product-card__price">200.000đ</span>
</div>
```

### 8.5. Dark Mode
- CoreUI hỗ trợ sẵn dark mode qua attribute `data-bs-theme="dark"`. Có thể thêm toggle để chuyển đổi.

## 9. Quy trình làm việc với Git

- **Nhánh chính:** `main` – luôn ổn định, đã deploy.
- **Nhánh phát triển:** `develop` – tích hợp các feature.
- **Quy tắc đặt tên nhánh feature:** `feature/USxx-ten-ngan` (vd: `feature/US01-auth`)
- **Commit message:** `[USxx] Mô tả ngắn gọn` (vd: `[US01] Implement register API`)
- **Pull Request:** Khi hoàn thành feature, tạo PR từ feature branch vào `develop`. PO sẽ review.
- **Sau mỗi Sprint:** Tạo tag `sprint-x`.

## 10. Định nghĩa "Xong" (Definition of Done)
- Code hoạt động trên môi trường local (FE + BE).
- Unit test cho các logic quan trọng (tính giá, xử lý order) đã được viết và pass.
- Tích hợp API thật (hoặc mock) thành công.
- Giao diện hiển thị đúng trên các trình duyệt (Chrome, Firefox) và responsive cơ bản.
- Code đã được review bởi PO và merge vào `develop`.
- Không có lỗi console hay cảnh báo nghiêm trọng.

## 11. Môi trường và Cấu hình

### Backend (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DigiXanh;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "your-secret-key",
    "Issuer": "DigiXanh",
    "Audience": "DigiXanhClient"
  },
  "Trefle": {
    "ApiKey": "your-trefle-api-key"
  },
  "VNPay": {
    "TmnCode": "your-tmn-code",
    "HashSecret": "your-hash-secret",
    "Url": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "https://your-fe-domain.com/payment-return"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:4200", "https://yourusername.github.io"]
  }
}
```

### Frontend (environment.ts)
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5001/api'
};
```
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://your-api.onrender.com/api'
};
```

## 12. Giao tiếp và Phối hợp
- Hai Agent có thể trao đổi trực tiếp (qua comment trong task hoặc chat) để thống nhất API contract, format dữ liệu, v.v.
- Mọi thay đổi về yêu cầu phải được PO xác nhận.
- Khi gặp khó khăn, hãy hỏi PO hoặc tìm giải pháp trong tài liệu chính thức của các công nghệ.

---

**Hãy đọc kỹ tài liệu này trước khi bắt đầu bất kỳ task nào. Nó sẽ là kim chỉ nam cho toàn bộ dự án.**

Chúc chúng ta làm việc hiệu quả và thành công! 🌿