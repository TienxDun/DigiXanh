Dưới đây là file **context.md** hoàn chỉnh, tích hợp tất cả các quyết định và thông tin chi tiết cho dự án DigiXanh. File này sẽ là tài liệu tham khảo duy nhất cho cả hai Agent (FE và BE) xuyên suốt dự án.

---

# 🌱 DigiXanh – Project Context (Dành cho AI Agents)

## 1. Tổng quan dự án
**DigiXanh** là website thương mại điện tử bán cây xanh, được xây dựng với mục đích học hỏi công nghệ và quy trình Scrum.

- **Product Owner (PO):** Bạn (ngườI dùng) – chịu trách nhiệm định hướng, ưu tiên backlog, review kết quả.
- **Development Team:** Hai AI agents – một phụ trách **Frontend (Angular)**, một phụ trách **Backend (ASP.NET Core)**.
- **Phương pháp:** Scrum (Sprints 1–2 tuần).
- **Mục tiêu cuốI cùng:** Hoàn thành phiên bản MVP với các chức năng CRUD của admin, flow mua hàng của khách, áp dụng 3 mẫu thiết kế (Adapter, Decorator, Facade) và triển khai miễn phí.

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

### Phase 2: Pre-deploy (Critical & High Priority)

| ID    | User Story                                                                 | Ghi chú | Ưu tiên |
|-------|-----------------------------------------------------------------------------|---------|---------|
| US19  | Xem lịch sử đơn hàng (User)                                                  |         | P0 - Critical |
| US20  | Xem chi tiết đơn hàng (User)                                                 |         | P0 - Critical |
| US21  | Quản lý đơn hàng (Admin) - Xem danh sách & cập nhật trạng thái              |         | P0 - Critical |
| US22  | Tìm kiếm và lọc cây (Public) - Theo tên, danh mục, khoảng giá               |         | P1 - High |
| US23  | Quản lý danh mục cây (Admin) - CRUD Category                                |         | P1 - High |
| US24  | Upload ảnh cây trực tiếp (thay vì chỉ URL)                                  |         | P1 - High |
| US25  | Quản lý tồn kho (Stock) - Cập nhật số lượng, hiển thị "Hết hàng"            |         | P1 - High |
| US26  | Validate tồn kho khi đặt hàng - Không cho đặt vượt stock                     |         | P1 - High |
| US27  | Xử lý callback VNPay (IPN) - Cập nhật trạng thái tự động                    |         | P1 - High |
| US28  | Thông báo lỗi thanh toán - Hiển thị lý do thất bại                          |         | P2 - Medium |

### Phase 3: Hoàn thiện hệ sinh thái (Medium Priority)

| ID    | User Story                                                                 | Ghi chú | Ưu tiên |
|-------|-----------------------------------------------------------------------------|---------|---------|
| US29  | Quản lý thông tin cá nhân (Profile) - Cập nhật tên, địa chỉ, SĐT            |         | P2 - Medium |
| US30  | Đổi mật khẩu                                                                 |         | P2 - Medium |
| US31  | Quản lý ngườI dùng (Admin) - Xem danh sách, khóa/mở khóa tài khoản          |         | P2 - Medium |
| US32  | Phân trang và sắp xếp sản phẩm - Theo giá, tên, ngày thêm                   |         | P2 - Medium |
| US33  | Hiển thị cây liên quan - Cùng danh mục ở trang chi tiết                     |         | P3 - Low |
| US34  | Audit log cho Admin - Lịch sử thay đổi trạng thái đơn hàng                  |         | P3 - Low |
| US35  | Trang lỗi 404/403 - Giao diện thân thiện cho lỗi không tìm thấy/không có quyền |     | P3 - Low |

### Phase 4: Nice to Have (Sau MVP)

| ID    | User Story                                                                 | Ghi chú | Ưu tiên |
|-------|-----------------------------------------------------------------------------|---------|---------|
| US36  | Đánh giá sản phẩm - User đánh giá sau khi nhận hàng                         |         | P4 - Future |
| US37  | Wishlist/Favorite - Lưu cây yêu thích                                       |         | P4 - Future |
| US38  | Thống kê nâng cao (Dashboard) - Biểu đồ doanh thu, top sản phẩm             |         | P4 - Future |
| US39  | Email thông báo - Xác nhận đặt hàng/thanh toán                              |         | P4 - Future |
| US40  | Dark Mode toggle - Chuyển đổi light/dark mode                               |         | P4 - Future |

*Lưu ý:* Các story có thể được chia nhỏ thêm trong quá trình Sprint Planning.

## 5. Database Schema (Optimized)

> **Cập nhật:** 2026-02-27 - Đã tối ưu với Indexes, Check Constraints và Audit Fields

### Tables

#### **AspNetUsers** (kế thừa IdentityUser)
| Column | Type | Constraints |
|--------|------|-------------|
| Id | nvarchar(450) | PK |
| FullName | nvarchar(200) | NOT NULL |
| Email | nvarchar(256) | Unique |
| UserName | nvarchar(256) | Unique |
| Address | nvarchar(500) | Nullable |
| PhoneNumber | nvarchar(max) | Nullable |
| CreatedAt | datetime2 | DEFAULT GETUTCDATE() |
| LastLoginAt | datetime2 | Nullable |

**Indexes:**
- `IX_AspNetUsers_CreatedAt` - Tối ưu query theo thờI gian tạo user

---

#### **Categories**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK, IDENTITY |
| Name | nvarchar(100) | NOT NULL |
| ParentCategoryId | int | FK → Categories.Id, Nullable (Self-referencing) |
| DisplayOrder | int | DEFAULT 0 |
| IsActive | bit | DEFAULT 1 |
| CreatedAt | datetime2 | DEFAULT GETUTCDATE() |
| UpdatedAt | datetime2 | Nullable |

**Indexes:**
- `IX_Categories_IsActive_DisplayOrder` - Query danh mục active theo thứ tự
- `IX_Categories_ParentCategoryId` - Query danh mục con

---

#### **Plants**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK, IDENTITY |
| Name | nvarchar(200) | NOT NULL |
| ScientificName | nvarchar(200) | NOT NULL |
| Description | nvarchar(max) | Nullable |
| Price | decimal(18,2) | CHECK >= 0 |
| CategoryId | int | FK → Categories.Id, Nullable, ON DELETE SET NULL |
| ImageUrl | nvarchar(500) | NOT NULL |
| TrefleId | int | Nullable |
| StockQuantity | int | Nullable, CHECK >= 0 |
| IsDeleted | bit | DEFAULT 0 (Soft Delete) |
| IsActive | bit | DEFAULT 1 |
| CreatedAt | datetime2 | DEFAULT GETUTCDATE() |
| UpdatedAt | datetime2 | Nullable |
| UpdatedBy | nvarchar(450) | Nullable |

**Indexes:**
- `IX_Plants_Filter_Sort` - (IsDeleted, IsActive, CreatedAt) - Query chính
- `IX_Plants_Name` - Tìm kiếm theo tên
- `IX_Plants_ScientificName` - Tìm kiếm theo tên khoa học
- `IX_Plants_TrefleId` - Filtered index WHERE TrefleId IS NOT NULL

**Check Constraints:**
- `CK_Plants_Price`: `[Price] >= 0`
- `CK_Plants_StockQuantity`: `[StockQuantity] IS NULL OR [StockQuantity] >= 0`

---

#### **CartItems**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK, IDENTITY |
| UserId | nvarchar(450) | FK → AspNetUsers.Id, ON DELETE CASCADE |
| PlantId | int | FK → Plants.Id, ON DELETE CASCADE |
| Quantity | int | CHECK > 0 |
| CreatedAt | datetime2 | DEFAULT GETUTCDATE() |
| UpdatedAt | datetime2 | DEFAULT GETUTCDATE() |
| ExpiresAt | datetime2 | Nullable |

**Indexes:**
- `IX_CartItems_UserId_PlantId` - UNIQUE - Tránh duplicate item trong giỏ
- `IX_CartItems_ExpiresAt` - Filtered index WHERE ExpiresAt IS NOT NULL (cleanup job)

**Check Constraints:**
- `CK_CartItems_Quantity`: `[Quantity] > 0`

---

#### **Orders**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK, IDENTITY |
| UserId | nvarchar(450) | FK → AspNetUsers.Id, ON DELETE RESTRICT |
| OrderDate | datetime2 | DEFAULT GETUTCDATE() |
| TotalAmount | decimal(18,2) | CHECK >= 0 |
| DiscountAmount | decimal(18,2) | CHECK >= 0 |
| FinalAmount | decimal(18,2) | CHECK >= 0 |
| Status | int | NOT NULL (enum: Pending, Paid, Shipped, Delivered, Cancelled) |
| RecipientName | nvarchar(200) | NOT NULL |
| Phone | nvarchar(20) | NOT NULL |
| ShippingAddress | nvarchar(500) | NOT NULL |
| PaymentMethod | int | NOT NULL (enum: Cash, VNPay) |
| TransactionId | nvarchar(100) | Nullable |
| PaymentUrl | nvarchar(2000) | Nullable |
| UpdatedAt | datetime2 | Nullable |
| UpdatedBy | nvarchar(450) | Nullable |

**Indexes:**
- `IX_Orders_UserId_OrderDate` - Query đơn hàng theo user
- `IX_Orders_Status` - Query theo trạng thái
- `IX_Orders_Status_OrderDate` - Query dashboard/admin
- `IX_Orders_TransactionId` - Filtered index WHERE TransactionId IS NOT NULL

**Check Constraints:**
- `CK_Orders_TotalAmount`: `[TotalAmount] >= 0`
- `CK_Orders_DiscountAmount`: `[DiscountAmount] >= 0`
- `CK_Orders_FinalAmount`: `[FinalAmount] >= 0`

---

#### **OrderItems**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK, IDENTITY |
| OrderId | int | FK → Orders.Id, ON DELETE CASCADE |
| PlantId | int | FK → Plants.Id, ON DELETE RESTRICT |
| Quantity | int | CHECK > 0 |
| UnitPrice | decimal(18,2) | CHECK >= 0 |
| CreatedAt | datetime2 | DEFAULT GETUTCDATE() |

**Indexes:**
- `IX_OrderItems_OrderId` - Query items theo đơn hàng
- `IX_OrderItems_PlantId` - Query thống kê sản phẩm

**Check Constraints:**
- `CK_OrderItems_Quantity`: `[Quantity] > 0`
- `CK_OrderItems_UnitPrice`: `[UnitPrice] >= 0`

---

#### **OrderStatusHistories** (Audit trail cho đơn hàng)
| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | PK, IDENTITY |
| OrderId | int | FK → Orders.Id, ON DELETE CASCADE |
| OldStatus | int | NOT NULL |
| NewStatus | int | NOT NULL |
| ChangedBy | nvarchar(450) | Nullable |
| Reason | nvarchar(500) | Nullable |
| ChangedAt | datetime2 | DEFAULT GETUTCDATE() |

**Indexes:**
- `IX_OrderStatusHistories_OrderId_ChangedAt` - Query lịch sử theo đơn hàng

---

### Migrations hiện có
1. `20260225114315_InitialIdentity` - Identity tables
2. `20260225160855_AddPlantAndCategory` - Plants và Categories
3. `20260225162034_SeedPlantsData` - Seed dữ liệu mẫu
4. `20260225165902_AddPlantDescriptionAndTrefleId` - Thêm mô tả và TrefleId
5. `20260225175932_AddOrdersForDashboard` - Thêm Orders và seed data
6. `20260226131828_AddCartItems` - Thêm CartItems
7. `20260226172016_US11_UpdateOrderSchema` - Cập nhật Order schema (US11)
8. `20260227162707_OptimizeDatabaseSchema` - **Tối ưu indexes, constraints, audit fields**

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
            // 7. Xóa giỏ hàng
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

- Git workflow do **PO quản lý** (branch/commit/push/PR/merge).
- Agent mặc định chỉ làm việc trên code local và báo cáo thay đổi sau khi hoàn thành task.
- Agent chỉ thao tác Git khi PO yêu cầu rõ ràng trong task.
- Nếu PO cần chuẩn hóa message, có thể dùng format: `[USxx] Mô tả ngắn gọn`.

## 10. Định nghĩa "Xong" (Definition of Done)
- Code hoạt động trên môi trường local (FE + BE).
- Unit test cho các logic quan trọng (tính giá, xử lý order) đã được viết và pass.
- Tích hợp API thật (hoặc mock) thành công.
- Giao diện hiển thị đúng trên các trình duyệt (Chrome, Firefox) và responsive cơ bản.
- Đã bàn giao rõ danh sách file sửa + cách test để PO tự commit/push/PR nếu cần.
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
