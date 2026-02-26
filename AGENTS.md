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
├── digixanh-fe/          # Frontend (Angular)
├── DigiXanh.API/         # Backend (ASP.NET Core 8 Web API)
├── DigiXanh.API.Tests/   # Unit tests cho Backend
├── .github/              # Tài liệu hướng dẫn và workflow
├── .agents/              # Agent rules và workflows
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
| External APIs | Trefle API (dữ liệu cây), VNPay Sandbox (thanh toán) |
| Testing | xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing |
| Hosting | Render (free plan) |

**Key Dependencies:**
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
│   ├── AuthController.cs
│   ├── PlantsController.cs
│   ├── CategoriesController.cs
│   ├── AdminPlantsController.cs
│   ├── AdminTrefleController.cs
│   ├── DashboardController.cs
│   └── TrefleController.cs
├── Services/
│   ├── Interfaces/ITrefleService.cs
│   └── Implementations/TrefleService.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Models/                    # Entity Framework entities
│   ├── ApplicationUser.cs
│   ├── Category.cs
│   ├── Plant.cs
│   └── Order.cs
├── DTOs/                      # Data Transfer Objects
│   ├── Auth/
│   ├── Categories/
│   ├── Common/
│   ├── Dashboard/
│   ├── Plants/
│   └── Trefle/
├── Constants/
│   └── DefaultRoles.cs        # "Admin", "User"
└── Migrations/                # EF Core migrations
```

### Frontend Architecture
```
src/app/
├── core/                      # Singleton services, guards, interceptors
│   ├── guards/admin.guard.ts
│   ├── interceptors/auth.interceptor.ts
│   ├── models/                # TypeScript interfaces
│   └── services/              # Auth, Plant, Dashboard services
├── layout/                    # Layout components
│   ├── default-layout/        # Admin layout (CoreUI sidebar/header)
│   └── public-layout/         # Public site layout
├── views/                     # Feature modules
│   ├── admin/                 # Plant management, dashboard
│   ├── auth/                  # Login, Register
│   ├── public/                # Home, plant list/detail
│   ├── cart/                  # Cart, checkout
│   └── base/                  # CoreUI base components
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
- API: `http://localhost:5000` hoặc `https://localhost:5001`
- Swagger UI: `http://localhost:5000/swagger`
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
Chạy file `quick-dev.cmd` để có menu tương tác:
```batch
quick-dev.cmd
```

Các tùy chọn:
- `[1]` Kiểm tra version (dotnet/node/npm)
- `[2]` BE - dotnet restore
- `[3]` BE - dotnet ef database update
- `[4]` BE - Thêm migration mới
- `[5]` BE - dotnet run
- `[6]` FE - npm install
- `[7]` FE - npm start
- `[8]` Chạy cả BE & FE (2 cửa sổ mới)
- `[9]` Chạy unit test backend
- `[A]` Mở Swagger

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
- `DigiXanh.API.Tests/Controllers/` - Integration tests cho controllers
- Sử dụng `WebApplicationFactory` và InMemory database

**Key Test Files:**
- `AdminAuthorizationTests.cs` - Kiểm tra phân quyền admin
- `AdminPlantsControllerTests.cs` - Tests CRUD cây
- `AuthControllerTests.cs` - Tests đăng nhập/đăng ký
- `DashboardControllerTests.cs` - Tests dashboard API

### Frontend Tests
```bash
cd digixanh-fe
npm test
```

- Sử dụng **Vitest** thay vì Karma/Jasmine
- Browser: Chromium Headless (qua Playwright)

---

## 6. Database

### Connection String (Development)
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DigiXanhDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

### Entities chính
| Entity | Mô tả |
|--------|-------|
| `ApplicationUser` | Kế thừa IdentityUser, thêm FullName |
| `Category` | Danh mục cây (Id, Name) |
| `Plant` | Thông tin cây (Name, ScientificName, Price, CategoryId, ImageUrl, IsDeleted...) |
| `Order` | Đơn hàng (UserId, TotalAmount, Status, ShippingAddress...) |
| `OrderItem` | Chi tiết đơn hàng |

### Migrations hiện có
1. `20260225114315_InitialIdentity` - Identity tables
2. `20260225160855_AddPlantAndCategory` - Plants và Categories
3. `20260225162034_SeedPlantsData` - Seed dữ liệu mẫu
4. `20260225165902_AddPlantDescriptionAndTrefleId` - Thêm mô tả và TrefleId
5. `20260225175932_AddOrdersForDashboard` - Thêm Orders và seed data

---

## 7. Authentication & Authorization

### JWT Configuration
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
- `User` - Người dùng thông thường

### Default Admin Account
```
Email: admin@digixanh.com
Password: Admin@123
```

### Protected Endpoints
- `[Authorize]` - Yêu cầu đăng nhập
- `[Authorize(Roles = "Admin")]` - Chỉ admin

---

## 8. API Endpoints

### Public (không cần auth)
- `GET /api/health` - Health check
- `GET /api/plants` - Danh sách cây (phân trang, tìm kiếm)
- `GET /api/plants/{id}` - Chi tiết cây
- `GET /api/categories` - Danh sách danh mục

### Auth
- `POST /api/auth/register` - Đăng ký
- `POST /api/auth/login` - Đăng nhập
- `GET /api/auth/me` - Thông tin user hiện tại

### Admin (cần role Admin)
- `GET /api/admin/plants` - Quản lý cây (phân trang, tìm kiếm, soft delete)
- `POST /api/admin/plants` - Thêm cây mới
- `PUT /api/admin/plants/{id}` - Cập nhật cây
- `DELETE /api/admin/plants/{id}` - Xóa cây (soft delete)
- `POST /api/admin/plants/bulk-soft-delete` - Xóa nhiều cây
- `GET /api/admin/trefle/search?q={query}` - Tìm kiếm cây từ Trefle API
- `GET /api/admin/trefle/{id}` - Chi tiết cây từ Trefle
- `GET /api/admin/dashboard` - Thống kê dashboard

---

## 9. Design Patterns (MVP Requirements)

### Adapter Pattern - Thanh toán
- Interface: `IPaymentAdapter`
- Implementations: `CashPaymentAdapter`, `VNPayPaymentAdapter`

### Decorator Pattern - Tính giá
- Interface: `IPriceCalculator`
- Base: `BasePriceCalculator`
- Decorator: `QuantityDiscountDecorator` (giảm 5% khi mua 2, 7% khi mua ≥3)

### Facade Pattern - Xử lý đơn hàng
- Class: `OrderProcessingFacade`
- Tích hợp: validate, tính giá, thanh toán, tạo order, xóa giỏ hàng

---

## 10. Tài liệu tham khảo bắt buộc

Khi nhận task, **PHẢI ĐỌC** các file sau:

| File | Đường dẫn | Dành cho |
|------|-----------|----------|
| Context | `.github/context.md` | Cả FE và BE |
| Backend Instruction | `.github/backend-instruction.md` | BE Agent |
| Frontend Instruction | `.github/frontend-instruction.md` | FE Agent |
| Copilot Instructions | `.github/copilot-instructions.md` | Cả hai |

---

## 11. Coding Conventions

### Backend (C#)
- Class/Interface: `PascalCase`
- Method/Property: `PascalCase`
- Variable/Parameter: `camelCase`
- Private field: `_camelCase`
- Constant: `UPPER_SNAKE_CASE`
- File name trùng với tên class

### Frontend (TypeScript/Angular)
- Class: `PascalCase` (e.g., `PlantListComponent`)
- File: `kebab-case` (e.g., `plant-list.component.ts`)
- Variable/Method: `camelCase`
- Observable suffix: `$` (e.g., `plants$`)
- Sử dụng `OnPush` change detection khi có thể
- Tách logic vào service, component giữ vai trò presentation

### CSS/SCSS (BEM Convention)
```scss
.block { }
.block__element { }
.block--modifier { }
```

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

### Backend (appsettings.Development.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DigiXanhDb;..."
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
  "Trefle": {
    "ApiKey": "usr-Rv-29a-iwQALB-hFIH2K8zZRi8agHad9ZsvSZa7216E"
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

- **Product Owner (PO):** Người dùng cung cấp task
- **Development Team:** 2 AI agents (FE + BE)
- **Tài liệu chính thức:**
  - CoreUI Angular: https://coreui.io/angular/docs/
  - Angular: https://angular.io/docs
  - ASP.NET Core: https://docs.microsoft.com/aspnet/core
  - Trefle API: https://docs.trefle.io/

---

*Cập nhật: 2026-02-26*
