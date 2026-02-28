# 🌱 DigiXanh - Tài liệu dành cho AI Agents

> **Lưu ý:** Đây là file tài liệu tổng hợp dành cho AI coding agents. Đọc kỹ file này trước khi làm việc với dự án.

---

## 1. Tổng quan dự án

**DigiXanh** là website thương mại điện tử bán cây xanh, được xây dựng theo mô hình tách biệt **Frontend** và **Backend**.

- **Mục đích:** Học hỏi công nghệ và quy trình Scrum
- **Phương pháp:** Scrum với Sprint 1-2 tuần
- **Mục tiêu MVP:** Hoàn thành các chức năng CRUD của admin, flow mua hàng của khách, áp dụng 3 mẫu thiết kế (Adapter, Decorator, Facade)

### Cấu trúc repository
```
DigiXanh/
├── digixanh-fe/          # Frontend (Angular 21+)
├── DigiXanh.API/         # Backend (ASP.NET Core 8 Web API)
├── DigiXanh.API.Tests/   # Unit tests cho Backend (xUnit)
├── .github/              # Tài liệu hướng dẫn và workflow
│   ├── context.md              # Product backlog và database schema
│   ├── backend-instruction.md  # Hướng dẫn chi tiết cho BE
│   ├── frontend-instruction.md # Hướng dẫn chi tiết cho FE
│   └── copilot-instructions.md # Hướng dẫn cho GitHub Copilot
├── .agents/              # Agent rules và workflows
├── scripts/              # PowerShell scripts (health check)
├── quick-dev.cmd         # Script khởi động nhanh BE+FE
└── DigiXanh.sln          # Solution file
```

---

## 2. Technology Stack

### Backend (DigiXanh.API)
| Thành phần | Công nghệ |
|------------|-----------|
| Framework | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core 8 (Code First) |
| Database | SQL Server (local dev) / MonsterASP.NET (production) |
| Authentication | ASP.NET Core Identity + JWT Bearer |
| External APIs | Perenual API (dữ liệu cây), VNPay Sandbox (thanh toán) |
| Testing | xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing |
| Hosting | Render (free plan) |

**Key Dependencies (DigiXanh.API.csproj):**
- `Microsoft.AspNetCore.Authentication.JwtBearer` (8.0.11)
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (8.0.11)
- `Microsoft.EntityFrameworkCore.SqlServer` (8.0.11)
- `AutoMapper.Extensions.Microsoft.DependencyInjection` (12.0.1)
- `Swashbuckle.AspNetCore` (6.6.2)
- `Newtonsoft.Json` (13.0.4)

### Frontend (digixanh-fe)
| Thành phần | Công nghệ |
|------------|-----------|
| Framework | Angular 21.1.5+ |
| UI Library | CoreUI for Angular (~5.6.15) |
| CSS Framework | Bootstrap 5 (qua CoreUI) |
| Form Handling | @ngx-formly/core + @ngx-formly/bootstrap |
| Icons | Font Awesome 6 Free |
| Charts | Chart.js + @coreui/angular-chartjs |
| Testing | Vitest 4.0.18 + jsdom + @vitest/browser-playwright |
| State Management | Services + BehaviorSubject (hoặc Signals) |
| Hosting | GitHub Pages |

**Node.js Requirements:** `^20.19.0 || ^22.12.0 || ^24.0.0`, npm `>=10`

---

## 3. Kiến trúc hệ thống

### Backend Architecture
```
Controllers → Services → (Repositories) → DbContext
                     ↗ Patterns (Adapter, Decorator, Facade)
```

**Thư mục chính:**
```
DigiXanh.API/
├── Controllers/               # API endpoints
│   ├── AuthController.cs           # Đăng ký, đăng nhập, profile
│   ├── PlantsController.cs         # Public API cho cây
│   ├── CategoriesController.cs     # Public API cho danh mục
│   ├── CartController.cs           # Quản lý giỏ hàng
│   ├── OrdersController.cs         # Đặt hàng, lịch sử đơn hàng
│   ├── PaymentController.cs        # Thanh toán VNPay
│   ├── UserController.cs           # Cập nhật profile ngườI dùng
│   ├── AdminPlantsController.cs    # Admin quản lý cây
│   ├── AdminCategoriesController.cs# Admin quản lý danh mục
│   ├── AdminOrdersController.cs    # Admin quản lý đơn hàng
│   ├── AdminPerenualController.cs  # Tìm kiếm cây từ Perenual API
│   └── DashboardController.cs      # Thống kê dashboard
├── Services/
│   ├── Interfaces/IPerenualService.cs, IOrderEmailService.cs
│   └── Implementations/PerenualService.cs, OrderEmailService.cs
├── Patterns/                  # Design Patterns
│   ├── Adapter/               # Thanh toán (Cash, VNPay)
│   ├── Decorator/             # Tính giá giảm dần
│   └── Facade/                # Xử lý đơn hàng
├── Data/
│   └── ApplicationDbContext.cs
├── Models/                    # Entity Framework entities
│   ├── ApplicationUser.cs     # Kế thừa IdentityUser + FullName, Address, Phone
│   ├── Category.cs            # Danh mục cây (hỗ trợ phân cấp)
│   ├── Plant.cs               # Thông tin cây
│   ├── CartItem.cs            # Giỏ hàng
│   ├── Order.cs               # Đơn hàng
│   ├── OrderItem.cs           # Chi tiết đơn hàng
│   └── OrderStatusHistory.cs  # Lịch sử trạng thái đơn hàng
├── DTOs/                      # Data Transfer Objects
│   ├── Auth/, Cart/, Categories/, Common/, Dashboard/, Orders/, Perenual/, Plants/
├── Constants/
│   └── DefaultRoles.cs        # "Admin", "User"
└── Migrations/                # EF Core migrations (8 migrations hiện có)
```

### Frontend Architecture
```
src/app/
├── core/                      # Singleton services, guards, interceptors
│   ├── guards/auth.guard.ts, admin.guard.ts
│   ├── interceptors/auth.interceptor.ts, error.interceptor.ts
│   ├── models/                # TypeScript interfaces
│   │   ├── plant.model.ts, cart.model.ts, order.model.ts
│   │   ├── dashboard.model.ts, pagination.model.ts
│   └── services/              # Services
│       ├── auth.service.ts, cart.service.ts, order.service.ts
│       ├── public-plant.service.ts, admin-plant.service.ts
│       ├── admin-category.service.ts, admin-dashboard.service.ts
│       ├── user.service.ts, toast.service.ts
│   └── utils/image-url.util.ts
├── layout/                    # Layout components
│   ├── default-layout/        # Admin layout (CoreUI sidebar/header)
│   │   ├── default-layout.component.ts
│   │   ├── default-header.component.ts
│   │   ├── default-footer.component.ts
│   │   └── _nav.ts            # Navigation config
│   └── public-layout/         # Public site layout
├── views/                     # Feature modules
│   ├── auth/                  # Login, Register
│   ├── public/                # Homepage, plant list/detail
│   ├── cart/                  # Cart, checkout, payment return
│   ├── orders/                # My orders, order detail
│   ├── admin/                 # Plant management, categories, orders
│   ├── user/                  # Profile page
│   └── dashboard/             # Admin dashboard
├── icons/                     # SVG icon definitions
├── app.config.ts              # App configuration (standalone)
└── app.routes.ts              # Route definitions
```

---

## 4. Build và Run Commands

### Backend
```bash
cd DigiXanh.API

# Restore packages
dotnet restore

# Apply database migrations
dotnet ef database update

# Run development server
dotnet run

# Add new migration
dotnet ef migrations add <MigrationName>
```

**Development URLs:**
- API: `https://localhost:5001` (chính) hoặc `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger`
- Health check: `GET /api/health`

### Frontend
```bash
cd digixanh-fe

# Install dependencies
npm install

# Start development server
npm start          # hoặc: ng serve -o

# Build for production
npm run build      # hoặc: ng build

# Run unit tests
npm test           # Vitest
```

**Development URL:** `http://localhost:4200`

### Quick Development Script
Chạy file `quick-dev.cmd` để khởi động cả BE và FE:
```batch
quick-dev.cmd
```

Script này sẽ:
1. Dừng các tiến trình cũ (dotnet, node)
2. Khởi động Backend
3. Đợi BE sẵn sàng (check `/api/health`)
4. Khởi động Frontend

### Chạy BE và FE riêng lẻ
```batch
# Chỉ chạy Backend
cd DigiXanh.API && dotnet run --launch-profile https

# Chỉ chạy Frontend  
cd digixanh-fe && npm start

# Kiểm tra BE health trước khi chạy FE
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\check-be-health.ps1 -SkipCertCheck
```

---

## 5. Testing

### Backend Tests
```bash
# Run all tests
dotnet test DigiXanh.sln

# Run with verbosity
dotnet test DigiXanh.sln --verbosity normal
```

**Test Projects:**
- `DigiXanh.API.Tests/` - Integration tests và unit tests

**Key Test Files:**
- `AdminAuthorizationTests.cs` - Kiểm tra phân quyền admin
- `AdminPlantsControllerTests.cs` - Tests CRUD cây
- `AuthControllerTests.cs` - Tests đăng nhập/đăng ký
- `CartControllerTests.cs` - Tests giỏ hàng
- `DashboardControllerTests.cs` - Tests dashboard API
- `OrderProcessingFacadeTests.cs` - Tests Facade pattern
- `QuantityDiscountDecoratorTests.cs` - Tests Decorator pattern
- `VNPayReturnProcessingTests.cs` - Tests thanh toán VNPay

### Frontend Tests
```bash
cd digixanh-fe
npm test
```

- Sử dụng **Vitest** thay vì Karma/Jasmine
- Browser: Chromium Headless (qua Playwright)
- Cấu hình trong `angular.json`:
```json
"test": {
  "builder": "@angular/build:unit-test",
  "options": {
    "tsConfig": "tsconfig.spec.json",
    "browsers": ["ChromiumHeadless"],
    "runnerConfig": true
  }
}
```

---

## 6. Database

### Connection String (Development)
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DigiXanhDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

### Entities chính
| Entity | Mô tả |
|--------|-------|
| `ApplicationUser` | Kế thừa IdentityUser, thêm FullName, Address, PhoneNumber, CreatedAt, LastLoginAt |
| `Category` | Danh mục cây (Id, Name, ParentCategoryId, DisplayOrder, IsActive) - hỗ trợ phân cấp |
| `Plant` | Thông tin cây (Name, ScientificName, Price, CategoryId, ImageUrl, StockQuantity, IsDeleted, IsActive) |
| `CartItem` | Giỏ hàng (UserId, PlantId, Quantity, ExpiresAt) |
| `Order` | Đơn hàng (UserId, TotalAmount, DiscountAmount, FinalAmount, Status, RecipientName, Phone, ShippingAddress, PaymentMethod, TransactionId) |
| `OrderItem` | Chi tiết đơn hàng (OrderId, PlantId, Quantity, UnitPrice) |
| `OrderStatusHistory` | Lịch sử trạng thái đơn hàng (OrderId, OldStatus, NewStatus, ChangedBy, Reason, ChangedAt) |

### Enums
```csharp
public enum OrderStatus { Pending, Paid, Processing, Shipped, Delivered, Cancelled }
public enum PaymentMethod { Cash, VNPay }
```

### Migrations hiện có (theo thứ tự)
1. `20260225114315_InitialIdentity` - Identity tables
2. `20260225160855_AddPlantAndCategory` - Plants và Categories
3. `20260225162034_SeedPlantsData` - Seed dữ liệu mẫu
4. `20260225165902_AddPlantDescriptionAndTrefleId` - Thêm mô tả
5. `20260225175932_AddOrdersForDashboard` - Thêm Orders và seed data
6. `20260226131828_AddCartItems` - Thêm CartItems
7. `20260226172016_US11_UpdateOrderSchema` - Cập nhật Order schema
8. `20260227162707_OptimizeDatabaseSchema` - Tối ưu indexes, constraints, audit fields

**Lưu ý:** Khi thay đổi model, tạo migration mới bằng:
```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

---

## 7. Authentication & Authorization

### JWT Configuration (appsettings.json)
```json
"Jwt": {
  "Key": "DigiXanh-Super-Secret-Key-For-Dev-Only-2026",
  "Issuer": "DigiXanh",
  "Audience": "DigiXanhClient",
  "ExpireMinutes": 60
}
```

### Roles
- `Admin` - Quản trị viên
- `User` - NgườI dùng thông thường

### Default Admin Account
```
Email: admin@digixanh.com
Password: Admin@123
FullName: DigiXanh Admin
```

Tài khoản admin được tự động seed khi khởi động ứng dụng (trong `Program.cs`).

### Protected Endpoints
- `[Authorize]` - Yêu cầu đăng nhập
- `[Authorize(Roles = "Admin")]` - Chỉ admin

---

## 8. API Endpoints

### Public (không cần auth)
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/health` | Health check |
| GET | `/api/plants` | Danh sách cây (phân trang, tìm kiếm, lọc) |
| GET | `/api/plants/{id}` | Chi tiết cây |
| GET | `/api/categories` | Danh sách danh mục |

### Auth
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| POST | `/api/auth/register` | Đăng ký |
| POST | `/api/auth/login` | Đăng nhập |
| GET | `/api/auth/me` | Thông tin user hiện tại |

### Cart (cần auth)
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/cart` | Lấy giỏ hàng |
| POST | `/api/cart/items` | Thêm vào giỏ |
| PUT | `/api/cart/items/{plantId}` | Cập nhật số lượng |
| DELETE | `/api/cart/items/{plantId}` | Xóa khỏi giỏ |

### Orders (cần auth)
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| POST | `/api/orders` | Tạo đơn hàng |
| GET | `/api/orders` | Lịch sử đơn hàng |
| GET | `/api/orders/{id}` | Chi tiết đơn hàng |
| POST | `/api/orders/{id}/cancel` | Hủy đơn hàng |

### Payment (cần auth)
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/payment/vnpay-return` | VNPay return URL |
| GET | `/api/payment/vnpay-ipn` | VNPay IPN |

### User (cần auth)
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/user/profile` | Lấy profile |
| PUT | `/api/user/profile` | Cập nhật profile |

### Admin (cần role Admin)
| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/api/admin/plants` | Quản lý cây (phân trang, tìm kiếm, soft delete) |
| POST | `/api/admin/plants` | Thêm cây mới |
| PUT | `/api/admin/plants/{id}` | Cập nhật cây |
| DELETE | `/api/admin/plants/{id}` | Xóa cây (soft delete) |
| POST | `/api/admin/plants/bulk-soft-delete` | Xóa nhiều cây |
| GET | `/api/admin/categories` | Quản lý danh mục |
| POST | `/api/admin/categories` | Thêm danh mục |
| PUT | `/api/admin/categories/{id}` | Cập nhật danh mục |
| DELETE | `/api/admin/categories/{id}` | Xóa danh mục |
| GET | `/api/admin/orders` | Quản lý đơn hàng |
| GET | `/api/admin/orders/{id}` | Chi tiết đơn hàng |
| PUT | `/api/admin/orders/{id}/status` | Cập nhật trạng thái đơn hàng |
| GET | `/api/admin/dashboard` | Thống kê dashboard |
| GET | `/api/admin/perenual/search?q={query}` | Tìm kiếm cây từ Perenual API |
| GET | `/api/admin/perenual/{id}` | Chi tiết cây từ Perenual |

---

## 9. Design Patterns (MVP Requirements)

### Adapter Pattern - Thanh toán
- **Interface:** `IPaymentAdapter`
- **Implementations:** 
  - `CashPaymentAdapter` - Thanh toán tiền mặt
  - `VNPayPaymentAdapter` - Thanh toán qua VNPay
- **Factory:** `IPaymentAdapterFactory` / `PaymentAdapterFactory`

### Decorator Pattern - Tính giá
- **Interface:** `IPriceCalculator`
- **Base:** `BasePriceCalculator` - Tính tổng cơ bản
- **Decorator:** `QuantityDiscountDecorator` - Giảm 5% khi mua ≥2sp, 7% khi mua ≥3sp (áp dụng mức cao nhất)

### Facade Pattern - Xử lý đơn hàng
- **Class:** `OrderProcessingFacade`
- **Tích hợp:** validate → tính giá → tạo order → thanh toán → gửi email → xóa giỏ hàng
- **Transaction:** Rollback nếu có lỗi

---

## 10. Tài liệu tham khảo bắt buộc

Khi nhận task, **PHẢI ĐỌC** các file sau:

| File | Đường dẫn | Dành cho |
|------|-----------|----------|
| Context | `.github/context.md` | Cả FE và BE - Product backlog, DB schema |
| Backend Instruction | `.github/backend-instruction.md` | BE Agent - Chi tiết BE |
| Frontend Instruction | `.github/frontend-instruction.md` | FE Agent - Chi tiết FE |
| Copilot Instructions | `.github/copilot-instructions.md` | Cả hai |

---

## 11. Coding Conventions

### Backend (C#)
| Loại | Quy ước | Ví dụ |
|------|---------|-------|
| Class/Interface | PascalCase | `PlantController`, `IPlantService` |
| Method/Property | PascalCase | `GetPlantsAsync()`, `UserName` |
| Variable/Parameter | camelCase | `plantId`, `userName` |
| Private field | _camelCase | `_dbContext` |
| Constant | UPPER_SNAKE_CASE | `DEFAULT_PAGE_SIZE` |
| File name | Trùng tên class | `Plant.cs`, `PlantController.cs` |

### Frontend (TypeScript/Angular)
| Loại | Quy ước | Ví dụ |
|------|---------|-------|
| Class | PascalCase | `PlantListComponent` |
| File | kebab-case | `plant-list.component.ts` |
| Variable/Method | camelCase | `getPlants()`, `userName` |
| Observable | Suffix `$` | `plants$`, `currentUser$` |
| Interface | PascalCase | `Plant`, `CartItem` |

**Angular Best Practices:**
- Sử dụng `OnPush` change detection khi có thể
- Tách logic vào service, component giữ vai trò presentation
- Sử dụng async pipe trong template thay vì subscribe thủ công
- Standalone components (không dùng NgModule)

### CSS/SCSS (BEM Convention)
```scss
.block { }
.block__element { }
.block--modifier { }
```

**Compact Premium UI Style:**
- Giảm white-space: dùng `p-2`, `p-3`, `mb-3` thay vì `py-5`, `mb-5`
- Typography nhỏ nhắn: `fs-3`, `fs-4`, `fs-5` thay vì `display-4`
- Ảnh nhỏ gọn: Giỏ hàng 56px, thẻ sản phẩm ~160px
- Bo góc mềm mại: `rounded-3`, `rounded-4` với `shadow-sm`

---

## 12. Git Workflow

> **QUAN TRỌNG:** Agent **KHÔNG TỰ TẠO NHÁNH/COMMIT/PUSH/PR** trừ khi PO yêu cầu rõ ràng.

- PO tự quản lý toàn bộ Git workflow
- Agent chỉ làm việc trên code local và báo cáo thay đổi
- Format commit message (nếu PO yêu cầu): `[USxx] Mô tả ngắn gọn`

---

## 13. Definition of Done (DoD)

Trước khi bàn giao task, kiểm tra:

- [ ] Code chạy đúng chức năng theo yêu cầu
- [ ] Đã viết unit test (nếu có) và tất cả đều pass
- [ ] Giao diện (FE) đúng thiết kế, responsive
- [ ] Không còn `console.log` hay code thừa
- [ ] Đã format code đúng chuẩn
- [ ] Đã ghi rõ hướng dẫn để PO tự commit/push nếu cần

---

## 14. Cấu hình môi trường

### Backend (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DigiXanhDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "DigiXanh-Super-Secret-Key-For-Dev-Only-2026",
    "Issuer": "DigiXanh",
    "Audience": "DigiXanhClient",
    "ExpireMinutes": 60
  },
  "AdminSeed": {
    "Email": "admin@digixanh.com",
    "Password": "Admin@123",
    "FullName": "DigiXanh Admin"
  },
  "Perenual": {
    "ApiKey": "your-perenual-api-key"
  },
  "VNPay": {
    "TmnCode": "your-tmn-code",
    "HashSecret": "your-hash-secret",
    "Url": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "https://localhost:5001/api/payment/vnpay-return",
    "FrontendReturnUrl": "http://localhost:4200/payment-return",
    "IpnUrl": "https://localhost:5001/api/payment/vnpay-ipn"
  }
}
```

### Frontend Proxy (proxy.conf.json)
```json
{
  "/api": {
    "target": "https://localhost:5001",
    "secure": false,
    "changeOrigin": true
  }
}
```

### Frontend Environment
```typescript
// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: '/api'  // Dùng proxy cho dev
};

// src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://your-api.onrender.com/api'
};
```

---

## 15. Phối hợp FE-BE

Khi task liên quan đến cả FE và BE:

1. **Liên hệ ngay** với Agent còn lại
2. **Thống nhất:**
   - API endpoint, method (GET/POST/PUT/DELETE)
   - Request/response format (DTO structure)
   - Error codes và handling
   - Authentication requirements
3. **Cùng test** trên local trước khi bàn giao

---

## 16. Liên hệ và Hỗ trợ

- **Product Owner (PO):** NgườI dùng cung cấp task
- **Development Team:** 2 AI agents (FE + BE)
- **Tài liệu chính thức:**
  - CoreUI Angular: https://coreui.io/angular/docs/
  - Angular: https://angular.io/docs
  - ASP.NET Core: https://docs.microsoft.com/aspnet/core
  - Perenual API: https://perenual.com/docs/api

---

*Cập nhật: 2026-02-28*
