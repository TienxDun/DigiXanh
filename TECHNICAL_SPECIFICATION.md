# 📋 ĐẶC TẢ KỸ THUẬT - DigiXanh E-Commerce Platform

**Phiên bản**: 1.0  
**Ngày cập nhật**: 03/2026  
**Trạng thái**: Active Development

---

## 📑 MỤC LỤC

1. [Tổng quan](#tổng-quan)
2. [Kiến trúc hệ thống](#kiến-trúc-hệ-thống)
3. [Tech Stack](#tech-stack)
4. [Các thành phần chính](#các-thành-phần-chính)
5. [Database Schema](#database-schema)
6. [API Endpoints](#api-endpoints)
7. [Design Patterns](#design-patterns)
8. [Bảo mật](#bảo-mật)
9. [Testing Strategy](#testing-strategy)
10. [Deployment](#deployment)

---

## 1. TỔNG QUAN

### 1.1 Mục đích dự án

**DigiXanh** là một nền tảng **thương mại điện tử chuyên biệt** bán các loại cây xanh trực tuyến, cung cấp:
- 🛍️ Catalog sản phẩm cây xanh với thông tin chi tiết
- 🛒 Hệ thống giỏ hàng và checkout
- 💳 Hỗ trợ 2 phương thức thanh toán: Cash (COD) & VNPay
- 📊 Admin Dashboard quản lý sản phẩm, đơn hàng, người dùng
- 📈 Analytics & reporting

### 1.2 Đối tượng người dùng

| Loại | Chức năng chính |
|------|-----------------|
| **Khách hàng (Customer)** | Duyệt cây, thêm giỏ hàng, checkout, theo dõi đơn hàng |
| **Admin** | Quản lý sản phẩm, danh mục, đơn hàng, người dùng, xem analytics |
| **System** | Tích hợp Perenual API để cập nhật dữ liệu cây xanh |

### 1.3 Scope chính

✅ **Bao gồm:**
- Product Management (CRUD)
- Shopping Cart & Checkout
- Order Management
- User Authentication & Authorization
- Payment Processing (Cash & VNPay)
- Admin Dashboard
- E-to-E & Unit Testing
- Performance Testing (K6)

❌ **Không bao gồm:**
- Gửi email thực tế (placeholder only)
- Multi-tenant support
- Mobile app native

---

## 2. KIẾN TRÚC HỆ THỐNG

### 2.1 Kiến trúc tổng thể

```
┌────────────────────────────────────────────────────────────────┐
│                        CLIENT LAYER                            │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Angular SPA (Customer Portal & Admin Dashboard)         │  │
│  │  - Customer Portal: Browse, Cart, Checkout               │  │
│  │  - Admin Dashboard: Product/Order/User Management        │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────┘
                              ↕️ HTTP/REST
┌────────────────────────────────────────────────────────────────┐
│                    API GATEWAY / MIDDLEWARE                    │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ CORS | Authentication (JWT) | Logging | Error Handling   │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────┘
                              ↕️
┌────────────────────────────────────────────────────────────────┐
│                    BUSINESS LOGIC LAYER                        │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ ASP.NET Core Controllers & Services                      │  │
│  │ ├─ AuthService          → JWT Token Management           │  │
│  │ ├─ PlantService         → Product CRUD                   │  │
│  │ ├─ OrderService         → Order Processing               │  │
│  │ ├─ CartService          → Shopping Cart Management       │  │
│  │ ├─ PaymentService       → Payment Processing             │  │
│  │ ├─ PerenualService      → External API Integration       │  │
│  │ └─ DashboardService     → Analytics & Reporting          │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────┘
                              ↕️
┌────────────────────────────────────────────────────────────────┐
│                    DATA ACCESS LAYER (EF Core)                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Repository Pattern + Unit of Work                        │  │
│  │ ├─ PlantRepository       → Plant CRUD Operations         │  │
│  │ ├─ OrderRepository       → Order CRUD Operations         │  │
│  │ ├─ UserRepository        → User CRUD Operations          │  │
│  │ └─ CategoryRepository    → Category CRUD Operations      │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────┘
                              ↕️
┌────────────────────────────────────────────────────────────────┐
│                    DATABASE LAYER                              │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ SQL Server 2022                                          │  │
│  │ ├─ Users (Identity)      → Authentication                │  │
│  │ ├─ Plants                → Product Catalog               │  │
│  │ ├─ Categories            → Product Categories            │  │
│  │ ├─ Orders                → Order Records                 │  │
│  │ ├─ OrderItems            → Order Line Items              │  │
│  │ ├─ CartItems             → Shopping Cart Items           │  │
│  │ └─ OrderStatusHistories  → Order Status Tracking         │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│                  EXTERNAL INTEGRATIONS                         │
│  ├─ Perenual API           → Plant Data Synchronization     │  │
│  ├─ VNPay Gateway          → Payment Processing             │  │
│  └─ Email Service          → Order Notifications            │  │
└────────────────────────────────────────────────────────────────┘
```

### 2.2 Mô hình giao tiếp

```
Frontend (Angular)
    │
    ├─→ GET /api/plants          → Lấy danh sách cây
    ├─→ POST /api/auth/login     → Đăng nhập
    ├─→ POST /api/cart/add       → Thêm vào giỏ
    ├─→ POST /api/orders         → Tạo đơn hàng
    ├─→ POST /api/payment/vnpay  → Thanh toán VNPay
    ├─→ GET /api/orders/{id}     → Chi tiết đơn hàng
    └─→ GET /api/dashboard/stats → Thống kê
    
Backend (ASP.NET Core)
    │
    ├─→ Database (SQL Server)    → Lưu trữ dữ liệu
    ├─→ Perenual API             → Cập nhật dữ liệu cây
    └─→ VNPay Gateway            → Xử lý thanh toán
```

---

## 3. TECH STACK

### 3.1 Frontend Stack

| Công nghệ | Phiên bản | Mục đích | Ghi chú |
|-----------|-----------|----------|--------|
| **Angular** | 21.1.5+ | Framework SPA | Modular, component-based architecture |
| **TypeScript** | 5.9 | Type-safe development | Strict mode enabled |
| **RxJS** | 7.8 | Reactive programming | Observables for async operations |
| **CoreUI** | 5.6 | Enterprise UI components | Ready-made admin template |
| **Chart.js** | 4.5.1 | Data visualization | Analytics dashboard |
| **NgxFormly** | 7.1.0 | Dynamic form generation | Flexible form handling |
| **Angular CDK** | 21.1.5 | Component dev kit | Advanced UI patterns |
| **Vitest** | 4.0 | Unit testing framework | Fast & modern |
| **Cypress** | 13 | E2E testing | Browser automation |
| **K6** | Latest | Performance testing | Load & stress testing |

### 3.2 Backend Stack

| Công nghệ | Phiên bản | Mục đích | Ghi chú |
|-----------|-----------|----------|--------|
| **ASP.NET Core** | 8.0 | Web API framework | Latest LTS version |
| **Entity Framework Core** | 8.0 | ORM & Code-First | Database abstraction |
| **SQL Server** | 2022 | Relational database | Enterprise-grade |
| **JWT Bearer** | 8.0.11 | Authentication | Token-based auth |
| **Identity** | 8.0.11 | User management | Built-in Identity system |
| **AutoMapper** | 12.0.1 | DTO mapping | Object transformation |
| **Swagger/OpenAPI** | 6.6.2 | API documentation | Interactive API docs |
| **xUnit** | 2.4 | Unit testing | Testing framework |
| **Newtonsoft.Json** | 13.0.4 | JSON serialization | Legacy JSON support |

### 3.3 DevOps & Deployment

| Công nghệ | Mục đích | Ghi chú |
|-----------|----------|--------|
| **GitHub Actions** | CI/CD Pipeline | Automated build & deploy |
| **Render** | Backend hosting | Cloud deployment (PaaS) |
| **GitHub Pages** | Frontend hosting | Static site hosting |
| **Docker** | Containerization | Backend deployment container |
| **SQL Server** | Database hosting | Cloud or on-premises |

---

## 4. CÁC THÀNH PHẦN CHÍNH

### 4.1 Frontend Architecture

#### 4.1.1 Cấu trúc thư mục

```
digixanh-fe/src/
├── app/
│   ├── core/              # Single-use services & guards
│   │   ├── guards/        # Route guards (auth, role-based)
│   │   ├── interceptors/  # HTTP interceptors
│   │   └── services/      # Core services (auth, user)
│   ├── shared/            # Reusable components & utilities
│   │   ├── components/    # Shared UI components
│   │   ├── pipes/         # Custom pipes
│   │   ├── directives/    # Custom directives
│   │   └── models/        # DTOs & interfaces
│   ├── layout/            # Layout components
│   │   ├── header/
│   │   ├── sidebar/
│   │   └── footer/
│   ├── views/             # Feature modules
│   │   ├── customer/      # Customer portal
│   │   │   ├── catalog/   # Product listing
│   │   │   ├── cart/      # Shopping cart
│   │   │   ├── checkout/  # Checkout process
│   │   │   └── orders/    # Order history
│   │   └── admin/         # Admin dashboard
│   │       ├── products/  # Product management
│   │       ├── orders/    # Order management
│   │       ├── users/     # User management
│   │       ├── categories/# Category management
│   │       └── dashboard/ # Analytics dashboard
│   ├── app.routes.ts      # Routing configuration
│   ├── app.config.ts      # App configuration
│   └── app.component.ts   # Root component
├── assets/                # Static assets
├── environments/          # Environment configs
└── scss/                  # Global styles
```

#### 4.1.2 Core Services

- **AuthService**: JWT token management, login/logout, user state
- **UserService**: User profile, role management
- **PlantService**: Plant CRUD operations, filtering, search
- **CartService**: Add/remove items, cart total calculation
- **OrderService**: Create order, order history, tracking
- **PaymentService**: Payment processing, transaction handling
- **DashboardService**: Analytics data, charts

#### 4.1.3 Key Features Implementation

**Product Catalog:**
- Search & filtering by category, price range, plant type
- Lazy loading for better performance
- Image optimization
- Sorting: popularity, price, newest

**Shopping Cart:**
- Real-time cart total calculation
- Item quantity management
- Persistent cart (localStorage)
- Session timeout handling

**Checkout Process:**
- Multi-step checkout form
- Address validation
- Payment method selection
- Order confirmation

**Admin Dashboard:**
- Product CRUD with bulk operations
- Order status management
- User management & role assignment
- Real-time analytics & charts
- Export data functionality

### 4.2 Backend Architecture

#### 4.2.1 Controllers (REST Endpoints)

| Controller | Trách nhiệm | Methods |
|-----------|-----------|---------|
| **AuthController** | Authentication & token management | `POST /register`, `POST /login`, `POST /refresh-token` |
| **PlantsController** | Public plant catalog | `GET /`, `GET /{id}`, `GET /search` |
| **AdminPlantsController** | Plant management (Admin only) | `POST`, `PUT`, `DELETE` |
| **CategoriesController** | Public categories | `GET /`, `GET /{id}` |
| **AdminCategoriesController** | Category management | `POST`, `PUT`, `DELETE` |
| **CartController** | Shopping cart operations | `GET`, `POST /add`, `DELETE /{id}`, `POST /clear` |
| **OrdersController** | Order operations (Customer) | `GET /`, `GET /{id}`, `POST`, `GET /{id}/status` |
| **AdminOrdersController** | Order management (Admin) | `GET /`, `PUT /{id}/status`, `DELETE /{id}` |
| **PaymentController** | Payment processing | `POST /vnpay/create`, `GET /vnpay/return` |
| **UserController** | User profile & management | `GET /profile`, `PUT /profile`, `GET /orders` |
| **AdminUsersController** | User management (Admin) | `GET /`, `PUT /{id}/role`, `DELETE /{id}` |
| **DashboardController** | Analytics & reports | `GET /stats`, `GET /revenue`, `GET /top-products` |
| **AdminPerenualController** | Sync Perenual data (Admin) | `POST /sync` |

#### 4.2.2 Service Layer

**Business Logic Services:**

```csharp
// Authentication & Authorization
AuthService
├─ Register user
├─ Authenticate user
├─ Generate JWT token
├─ Validate refresh token
└─ Handle password reset

// Product Management
PlantService
├─ Get all plants with pagination
├─ Get plant by ID
├─ Search & filter plants
├─ Create/Update/Delete plant
├─ Manage stock quantity
└─ Soft delete implementation

// Shopping Cart
CartService
├─ Add item to cart
├─ Remove item from cart
├─ Update item quantity
├─ Calculate cart total
├─ Clear cart
└─ Convert cart to order

// Order Processing
OrderService
├─ Create order from cart
├─ Get order by ID
├─ Get user orders
├─ Update order status
├─ Track order
├─ Handle order cancellation
└─ Calculate discounts

// Payment Integration
PaymentService (Adapter Pattern)
├─ CashPaymentAdapter    → COD payment handler
├─ VNPayPaymentAdapter   → VNPay payment handler
└─ IPaymentAdapterFactory → Factory for payment adapters

// External API Integration
PerenualService
├─ Fetch plant data from Perenual API
├─ Sync plant information
└─ Handle API errors gracefully

// Dashboard & Analytics
DashboardService
├─ Get sales statistics
├─ Get revenue reports
├─ Get top selling products
├─ Get order statistics
└─ Generate chart data
```

#### 4.2.3 Data Access Layer (Repository Pattern)

```csharp
IRepository<T> interface
├─ GetAll()
├─ GetById(id)
├─ GetAsync(predicate)
├─ Add(entity)
├─ Update(entity)
├─ Delete(entity)
└─ SaveChangesAsync()

Implementations:
├─ PlantRepository
├─ OrderRepository
├─ UserRepository
├─ CategoryRepository
└─ CartItemRepository
```

#### 4.2.4 Middleware & Filters

- **JWT Authentication Middleware**: Token validation
- **CORS Middleware**: Cross-origin requests handling
- **Error Handling Middleware**: Global exception handling
- **Logging Middleware**: Request/response logging
- **Authorization Filters**: Role-based access control

---

## 5. DATABASE SCHEMA

### 5.1 Entity Relationship Diagram

```
┌─────────────────┐
│   AspNetUsers   │ (Identity)
├─────────────────┤
│ Id (PK)         │
│ UserName        │ 1
│ Email           │ │
│ PasswordHash    │ │
│ CreatedAt       │ │
│ UpdatedAt       │ │
└─────────────────┘
    │ 1
    │
    ├────────────────────┬─────────────┬──────────────┐
    │ N                  │ N           │ N            │
    ↓                    ↓             ↓              ↓
┌────────────┐   ┌──────────────┐  ┌─────────┐  ┌────────────┐
│  Orders    │   │   CartItems  │  │ Reviews │  │ Favorites  │
└────────────┘   └──────────────┘  └─────────┘  └────────────┘
    │
    │ 1
    │
    ├─── N ──→ ┌────────────┐
              │ OrderItems │
              └────────────┘
                    │
                    │ N
                    ↓
              ┌────────────┐
              │   Plants   │ ← Referenced by Orders, Cart, Reviews
              └────────────┘
                    │
                    │ N
                    ↓
              ┌──────────────┐
              │  Categories  │
              └──────────────┘

┌──────────────────────┐
│ OrderStatusHistories │ (Audit)
└──────────────────────┘
       │
       │ N
       ↓
    Orders (1:Many relationship)
```

### 5.2 Bảng chi tiết

#### **Users (AspNetUsers - Identity)**
```sql
CREATE TABLE AspNetUsers
(
    Id                   NVARCHAR(450) PRIMARY KEY,
    UserName             NVARCHAR(256),
    Email                NVARCHAR(256),
    PasswordHash         NVARCHAR(MAX),
    PhoneNumber          NVARCHAR(20),
    FullName             NVARCHAR(256),
    Address              NVARCHAR(500),
    ProfilePictureUrl    NVARCHAR(MAX),
    CreatedAt            DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt            DATETIME2,
    UpdatedBy            NVARCHAR(450),
    IsActive             BIT DEFAULT 1,
    IsDeleted            BIT DEFAULT 0
);
```

#### **Categories**
```sql
CREATE TABLE Categories
(
    Id          INT PRIMARY KEY IDENTITY(1,1),
    Name        NVARCHAR(100) UNIQUE NOT NULL,
    Description NVARCHAR(MAX),
    ImageUrl    NVARCHAR(MAX),
    IsActive    BIT DEFAULT 1,
    IsDeleted   BIT DEFAULT 0,
    CreatedAt   DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt   DATETIME2,
    UpdatedBy   NVARCHAR(450)
);
```

#### **Plants**
```sql
CREATE TABLE Plants
(
    Id              INT PRIMARY KEY IDENTITY(1,1),
    Name            NVARCHAR(255) NOT NULL,
    ScientificName  NVARCHAR(255),
    Description     NVARCHAR(MAX),
    Price           DECIMAL(10,2) NOT NULL,
    CategoryId      INT FOREIGN KEY REFERENCES Categories(Id),
    ImageUrl        NVARCHAR(MAX) NOT NULL,
    StockQuantity   INT,
    IsActive        BIT DEFAULT 1,
    IsDeleted       BIT DEFAULT 0,
    TrefleId        INT,              -- External ID from Perenual API
    CreatedAt       DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2,
    UpdatedBy       NVARCHAR(450)
);

CREATE INDEX IX_Plants_Category ON Plants(CategoryId);
CREATE INDEX IX_Plants_Price ON Plants(Price);
CREATE INDEX IX_Plants_IsActive ON Plants(IsActive);
```

#### **Orders**
```sql
CREATE TABLE Orders
(
    Id               INT PRIMARY KEY IDENTITY(1,1),
    UserId           NVARCHAR(450) FOREIGN KEY REFERENCES AspNetUsers(Id),
    OrderDate        DATETIME2 DEFAULT GETUTCDATE(),
    TotalAmount      DECIMAL(10,2) NOT NULL,
    DiscountAmount   DECIMAL(10,2) DEFAULT 0,
    FinalAmount      DECIMAL(10,2) NOT NULL,
    
    ShippingAddress  NVARCHAR(500) NOT NULL,
    RecipientName    NVARCHAR(255) NOT NULL,
    Phone            NVARCHAR(20) NOT NULL,
    
    Status           INT DEFAULT 0,  -- 0:Pending, 1:Paid, 2:Shipped, 3:Delivered, 4:Cancelled
    PaymentMethod    INT DEFAULT 0,  -- 0:Cash, 1:VNPay
    TransactionId    NVARCHAR(255),   -- VNPay transaction ID
    PaymentUrl       NVARCHAR(MAX),   -- VNPay payment gateway URL
    
    UpdatedAt        DATETIME2,
    UpdatedBy        NVARCHAR(450)
);

CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_Status ON Orders(Status);
CREATE INDEX IX_Orders_OrderDate ON Orders(OrderDate DESC);
```

#### **OrderItems**
```sql
CREATE TABLE OrderItems
(
    Id       INT PRIMARY KEY IDENTITY(1,1),
    OrderId  INT FOREIGN KEY REFERENCES Orders(Id) ON DELETE CASCADE,
    PlantId  INT FOREIGN KEY REFERENCES Plants(Id),
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    Subtotal DECIMAL(10,2) NOT NULL  -- Quantity * UnitPrice
);

CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
CREATE INDEX IX_OrderItems_PlantId ON OrderItems(PlantId);
```

#### **CartItems**
```sql
CREATE TABLE CartItems
(
    Id       INT PRIMARY KEY IDENTITY(1,1),
    UserId   NVARCHAR(450) FOREIGN KEY REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    PlantId  INT FOREIGN KEY REFERENCES Plants(Id),
    Quantity INT NOT NULL,
    AddedAt  DATETIME2 DEFAULT GETUTCDATE()
);

CREATE INDEX IX_CartItems_UserId ON CartItems(UserId);
CREATE UNIQUE INDEX UX_CartItems_User_Plant ON CartItems(UserId, PlantId);
```

#### **OrderStatusHistories** (Audit Trail)
```sql
CREATE TABLE OrderStatusHistories
(
    Id          INT PRIMARY KEY IDENTITY(1,1),
    OrderId     INT FOREIGN KEY REFERENCES Orders(Id) ON DELETE CASCADE,
    OldStatus   INT,
    NewStatus   INT,
    ChangedAt   DATETIME2 DEFAULT GETUTCDATE(),
    ChangedBy   NVARCHAR(450),
    Reason      NVARCHAR(MAX)
);

CREATE INDEX IX_StatusHistories_OrderId ON OrderStatusHistories(OrderId);
CREATE INDEX IX_StatusHistories_ChangedAt ON OrderStatusHistories(ChangedAt DESC);
```

### 5.3 Constraints & Rules

- **Plant Price**: Must be > 0
- **Stock Quantity**: >= 0, null = unlimited
- **Order Amount**: Calculated from OrderItems
- **Discount**: Cannot exceed TotalAmount
- **Cart**: One entry per User-Plant combination
- **Soft Delete**: IsDeleted flag, logical delete
- **Timestamps**: CreatedAt (immutable), UpdatedAt (auto-update)
- **User Requirements**: Unique email per system

---

## 6. API ENDPOINTS

### 6.1 Authentication Endpoints

#### POST `/api/auth/register`
```json
Request Body:
{
  "email": "user@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "fullName": "John Doe",
  "phoneNumber": "0912345678"
}

Response: 200 OK
{
  "message": "Registration successful",
  "userId": "uuid-string"
}
```

#### POST `/api/auth/login`
```json
Request Body:
{
  "email": "user@example.com",
  "password": "Password123!"
}

Response: 200 OK
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh-token-string",
  "expiresIn": 3600,
  "user": {
    "id": "user-id",
    "email": "user@example.com",
    "fullName": "John Doe",
    "roles": ["User"]
  }
}
```

#### POST `/api/auth/refresh-token`
```json
Request Body:
{
  "refreshToken": "refresh-token-string"
}

Response: 200 OK
{
  "token": "new-jwt-token",
  "expiresIn": 3600
}
```

### 6.2 Products (Plants) Endpoints

#### GET `/api/plants`
**Query Parameters:**
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10)
- `categoryId` (int): Filter by category
- `minPrice` (decimal): Minimum price filter
- `maxPrice` (decimal): Maximum price filter
- `search` (string): Search by plant name
- `sortBy` (string): "price", "name", "newest"

```json
Response: 200 OK
{
  "data": [
    {
      "id": 1,
      "name": "Money Plant",
      "scientificName": "Epipremnum aureum",
      "price": 150000,
      "categoryId": 1,
      "imageUrl": "https://example.com/image.jpg",
      "stockQuantity": 50,
      "isActive": true
    }
  ],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 10
}
```

#### GET `/api/plants/{id}`
```json
Response: 200 OK
{
  "id": 1,
  "name": "Money Plant",
  "scientificName": "Epipremnum aureum",
  "description": "Popular indoor plant...",
  "price": 150000,
  "category": { "id": 1, "name": "Indoor Plants" },
  "imageUrl": "https://example.com/image.jpg",
  "stockQuantity": 50,
  "trefleId": 123456,
  "createdAt": "2026-01-15T10:30:00Z"
}
```

#### POST `/api/admin/plants` **(Admin only)**
```json
Request Body:
{
  "name": "New Plant",
  "scientificName": "Scientific name",
  "description": "Description",
  "price": 200000,
  "categoryId": 1,
  "imageUrl": "url-to-image",
  "stockQuantity": 100
}

Response: 201 Created
```

#### PUT `/api/admin/plants/{id}` **(Admin only)**
```json
Request Body:
{
  "name": "Updated Plant",
  "price": 250000,
  "stockQuantity": 80
}

Response: 204 No Content
```

#### DELETE `/api/admin/plants/{id}` **(Admin only)**
```
Response: 204 No Content (Soft delete)
```

### 6.3 Categories Endpoints

#### GET `/api/categories`
```json
Response: 200 OK
{
  "data": [
    {
      "id": 1,
      "name": "Indoor Plants",
      "description": "Perfect for indoor spaces",
      "imageUrl": "https://example.com/image.jpg"
    }
  ]
}
```

#### POST `/api/admin/categories` **(Admin only)**
```json
Request Body:
{
  "name": "New Category",
  "description": "Description",
  "imageUrl": "url"
}

Response: 201 Created
```

### 6.4 Shopping Cart Endpoints

#### GET `/api/cart` **(Authenticated)**
```json
Response: 200 OK
{
  "items": [
    {
      "id": 1,
      "plantId": 1,
      "plantName": "Money Plant",
      "price": 150000,
      "quantity": 2,
      "subtotal": 300000
    }
  ],
  "totalItems": 2,
  "totalAmount": 300000
}
```

#### POST `/api/cart/add` **(Authenticated)**
```json
Request Body:
{
  "plantId": 1,
  "quantity": 2
}

Response: 200 OK
{
  "message": "Item added to cart",
  "cartTotal": 300000
}
```

#### DELETE `/api/cart/{itemId}` **(Authenticated)**
```
Response: 204 No Content
```

#### POST `/api/cart/clear` **(Authenticated)**
```
Response: 204 No Content
```

### 6.5 Orders Endpoints

#### POST `/api/orders` **(Authenticated)**
```json
Request Body:
{
  "shippingAddress": "123 Main St, City",
  "recipientName": "John Doe",
  "phone": "0912345678",
  "paymentMethod": 0  // 0: Cash, 1: VNPay
}

Response: 201 Created
{
  "orderId": 1,
  "paymentUrl": null or "vnpay-gateway-url"
}
```

#### GET `/api/orders` **(Authenticated)**
```json
Response: 200 OK
{
  "data": [
    {
      "id": 1,
      "orderDate": "2026-03-01T10:30:00Z",
      "totalAmount": 300000,
      "finalAmount": 300000,
      "status": "Pending",
      "paymentMethod": "Cash",
      "itemCount": 2
    }
  ]
}
```

#### GET `/api/orders/{id}` **(Authenticated)**
```json
Response: 200 OK
{
  "id": 1,
  "orderDate": "2026-03-01T10:30:00Z",
  "totalAmount": 300000,
  "discountAmount": 0,
  "finalAmount": 300000,
  "status": "Pending",
  "shippingAddress": "123 Main St",
  "recipientName": "John Doe",
  "phone": "0912345678",
  "paymentMethod": "Cash",
  "items": [
    {
      "plantName": "Money Plant",
      "quantity": 2,
      "unitPrice": 150000,
      "subtotal": 300000
    }
  ],
  "statusHistories": [
    {
      "oldStatus": null,
      "newStatus": "Pending",
      "changedAt": "2026-03-01T10:30:00Z"
    }
  ]
}
```

#### PUT `/api/admin/orders/{id}/status` **(Admin only)**
```json
Request Body:
{
  "newStatus": "Shipped",
  "reason": "Order shipped with tracking number XYZ"
}

Response: 204 No Content
```

### 6.6 Payment Endpoints

#### POST `/api/payment/vnpay/create` **(Authenticated)**
```json
Request Body:
{
  "orderId": 1,
  "returnUrl": "https://frontend.com/payment-return"
}

Response: 200 OK
{
  "paymentUrl": "https://sandbox.vnpayment.vn/paygate?vnp_TxnRef=..."
}
```

#### GET `/api/payment/vnpay/return`
```
Query Parameters:
- vnp_TxnRef
- vnp_ResponseCode
- vnp_TransactionNo
- etc.

Response: 200 OK (Redirect to frontend success/failure page)
```

### 6.7 Admin Dashboard Endpoints

#### GET `/api/dashboard/stats` **(Admin only)**
```json
Response: 200 OK
{
  "totalRevenue": 5000000,
  "totalOrders": 150,
  "totalCustomers": 120,
  "totalProducts": 85,
  "revenueThisMonth": 2000000,
  "ordersThisMonth": 45,
  "newCustomersThisMonth": 20
}
```

#### GET `/api/dashboard/revenue` **(Admin only)**
```json
Query Parameters:
- startDate (ISO 8601)
- endDate (ISO 8601)
- groupBy: "daily", "weekly", "monthly"

Response: 200 OK
{
  "data": [
    {
      "date": "2026-03-01",
      "revenue": 1200000,
      "orderCount": 15
    }
  ]
}
```

#### GET `/api/dashboard/top-products` **(Admin only)**
```json
Query Parameters:
- limit (default: 10)

Response: 200 OK
{
  "data": [
    {
      "productId": 1,
      "productName": "Money Plant",
      "unitsSold": 150,
      "revenue": 22500000
    }
  ]
}
```

### 6.8 User Management Endpoints

#### GET `/api/user/profile` **(Authenticated)**
```json
Response: 200 OK
{
  "id": "user-id",
  "email": "user@example.com",
  "fullName": "John Doe",
  "phoneNumber": "0912345678",
  "address": "123 Main St, City"
}
```

#### PUT `/api/user/profile` **(Authenticated)**
```json
Request Body:
{
  "fullName": "Updated Name",
  "phoneNumber": "0987654321",
  "address": "New Address"
}

Response: 204 No Content
```

#### GET `/api/admin/users` **(Admin only)**
```json
Response: 200 OK
{
  "data": [
    {
      "id": "user-id",
      "email": "user@example.com",
      "fullName": "John Doe",
      "roles": ["User"],
      "createdAt": "2026-01-01T00:00:00Z",
      "isActive": true
    }
  ]
}
```

#### PUT `/api/admin/users/{id}/role` **(Admin only)**
```json
Request Body:
{
  "roleName": "Admin"
}

Response: 204 No Content
```

### 6.9 External API Sync

#### POST `/api/admin/perenual/sync` **(Admin only)**
```json
Request Body:
{
  "pageNumber": 1
}

Response: 200 OK
{
  "message": "Sync started",
  "plantsAdded": 50,
  "plantsUpdated": 20
}
```

---

## 7. DESIGN PATTERNS

### 7.1 Adapter Pattern (Payment Processing)

**Mục đích**: Cung cấp interface thống nhất cho nhiều phương thức thanh toán.

```csharp
// Common interface
public interface IPaymentAdapter
{
    Task<PaymentResponse> ProcessPayment(PaymentRequest request);
    Task<PaymentStatus> CheckPaymentStatus(string transactionId);
}

// Implementations
public class CashPaymentAdapter : IPaymentAdapter
{
    // Handle cash-on-delivery payments
}

public class VNPayPaymentAdapter : IPaymentAdapter
{
    // Handle VNPay gateway integration
}

// Factory
public interface IPaymentAdapterFactory
{
    IPaymentAdapter GetAdapter(PaymentMethod method);
}

// Usage in OrderService
var adapter = _paymentAdapterFactory.GetAdapter(order.PaymentMethod);
var response = await adapter.ProcessPayment(paymentRequest);
```

**Benefit**: 
- ✅ Dễ thêm phương thức thanh toán mới (Stripe, Paypal, etc.)
- ✅ Không cần thay đổi core logic
- ✅ Testable với mock adapters

### 7.2 Decorator Pattern (Discount Calculation)

**Mục đích**: Tính toán discount một cách linh hoạt.

```csharp
// Base decorator
public interface IDiscount
{
    decimal CalculateDiscount(decimal amount);
}

// Decorators
public class CategoryDiscount : IDiscount
{
    private readonly IDiscount _innerDiscount;
    public decimal CalculateDiscount(decimal amount) => ...;
}

public class LoyaltyDiscount : IDiscount
{
    private readonly IDiscount _innerDiscount;
    public decimal CalculateDiscount(decimal amount) => ...;
}

// Usage - stack multiple discounts
var discount = new CategoryDiscount(
    new LoyaltyDiscount(
        new BaseDiscount()
    )
);
var totalDiscount = discount.CalculateDiscount(amount);
```

**Benefit**:
- ✅ Stackable discount rules
- ✅ Runtime configuration
- ✅ Flexible combination

### 7.3 Facade Pattern (Order Processing)

**Mục đích**: Đơn giản hóa quy trình tạo đơn hàng phức tạp.

```csharp
public class OrderProcessingFacade
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly IInventoryService _inventoryService;

    public async Task<OrderResult> CreateAndProcessOrder(
        string userId, 
        OrderRequest request)
    {
        // 1. Validate cart
        var cartItems = await _cartService.GetCart(userId);
        
        // 2. Validate inventory
        await _inventoryService.ValidateStock(cartItems);
        
        // 3. Create order
        var order = await _orderService.CreateOrder(userId, request);
        
        // 4. Process payment
        if (request.PaymentMethod == PaymentMethod.VNPay)
        {
            var paymentUrl = await _paymentService.GeneratePaymentUrl(order);
            order.PaymentUrl = paymentUrl;
        }
        
        // 5. Clear cart
        await _cartService.ClearCart(userId);
        
        // 6. Notify user
        await _notificationService.SendOrderConfirmation(order);
        
        return new OrderResult { Order = order, Success = true };
    }
}

// Usage in Controller
var result = await _orderProcessingFacade.CreateAndProcessOrder(userId, request);
```

**Benefit**:
- ✅ Single entry point for complex workflow
- ✅ Reduced client-side complexity
- ✅ Centralized business logic
- ✅ Easy to maintain & modify workflow

### 7.4 Repository Pattern (Data Access)

**Mục đích**: Abstract data access logic.

```csharp
public interface IRepository<T> where T : class
{
    IQueryable<T> GetAll();
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task SaveChangesAsync();
}

public class GenericRepository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;
    
    // Implementation...
}

// Usage
var plants = await _plantRepository
    .FindAsync(p => p.IsActive && p.CategoryId == categoryId);
```

**Benefit**:
- ✅ Testable with mocks
- ✅ Decoupled from EF Core
- ✅ Reusable across entities

### 7.5 Unit of Work Pattern (Transaction Management)

```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<Plant> Plants { get; }
    IRepository<Order> Orders { get; }
    IRepository<CartItem> CartItems { get; }
    
    Task SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    
    public IRepository<Plant> Plants => _plants ??= 
        new PlantRepository(_context);
    
    // Transaction management...
}

// Usage
using (var uow = _unitOfWorkFactory.Create())
{
    try
    {
        await uow.BeginTransactionAsync();
        await uow.Orders.AddAsync(order);
        await uow.CartItems.DeleteAsync(cartItem);
        await uow.SaveChangesAsync();
        await uow.CommitTransactionAsync();
    }
    catch
    {
        await uow.RollbackTransactionAsync();
        throw;
    }
}
```

**Benefit**:
- ✅ Transaction integrity
- ✅ ACID compliance
- ✅ Atomic operations

---

## 8. BẢO MẬT

### 8.1 Authentication & Authorization

#### JWT Token Strategy
- **Token Type**: Bearer JWT
- **Payload Claims**:
  - `sub` (subject): User ID
  - `email`: User email
  - `roles`: User roles
  - `iat`: Issued at
  - `exp`: Expiration (1 hour)

- **Refresh Token**:
  - Stored in database (hashed)
  - Rotation on each use
  - Expiration: 7 days

```csharp
// Token Generation
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id),
    new Claim(ClaimTypes.Email, user.Email),
    new(ClaimTypes.Role, role)
};

var token = new JwtSecurityToken(
    issuer: _jwtConfig["Issuer"],
    audience: _jwtConfig["Audience"],
    claims: claims,
    expires: DateTime.UtcNow.AddHours(1),
    signingCredentials: new SigningCredentials(
        new SymmetricSecurityKey(key),
        SecurityAlgorithms.HmacSha256)
);
```

#### Role-Based Access Control (RBAC)
```
Roles:
├─ Admin      → Full system access
├─ Manager    → Product & Order management
└─ User       → Read catalog, manage own orders

Authorization Filters:
├─ [Authorize]                    → Require authentication
├─ [Authorize(Roles = "Admin")]   → Role-specific
└─ [AllowAnonymous]               → Public access
```

### 8.2 Vulnerabilities Prevention

| Vulnerability | Prevention | Implementation |
|--|--|--|
| **XSS** | Input validation & output encoding | Angular sanitization, Content-Security-Policy header |
| **CSRF** | Anti-CSRF tokens | ASP.NET Core anti-forgery middleware |
| **SQL Injection** | Parameterized queries | Entity Framework Core only |
| **CORS** | Explicit origin whitelist | Policy-based CORS |
| **Password** | Hashing (bcrypt via Identity) | ASP.NET Identity password hashers |
| **Token Leakage** | HTTPS only, HttpOnly cookies | Secure JWT transmission |
| **Brute Force** | Rate limiting | Custom middleware for login attempts |
| **Sensitive Data** | No logging of passwords/tokens | Logging configuration |

### 8.3 CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
            "http://localhost:4200",
            "https://localhost:4200",
            "https://tienxdun.github.io"  // Production frontend
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});
```

### 8.4 Data Protection

- **Soft Delete**: IsDeleted flag for data retention
- **Audit Trail**: OrderStatusHistories for transaction tracking
- **Encryption**: Sensitive data at rest (if required)
- **PII Protection**: Phone number, address - minimal logging

### 8.5 API Security Headers

```
- Content-Security-Policy: Prevent XSS
- X-Content-Type-Options: nosniff
- X-Frame-Options: DENY
- Strict-Transport-Security: HSTS for HTTPS
```

---

## 9. TESTING STRATEGY

### 9.1 Testing Pyramid

```
        △
       /|\
      / | \    E2E Tests (User workflows)
     /  |  \   - 16 critical paths
    /   |   \  - Cypress
   /____|____\ 

      /\
     /  \     Integration Tests (API + DB)
    /    \    - API endpoints
   /______\   - Data consistency

  /        \  Unit Tests (Business logic)
 /          \ - Services, Repositories
/____________\- xUnit, Vitest
```

### 9.2 Unit Testing

**Backend (xUnit + Moq)**
```csharp
[Fact]
public async Task CreateOrder_ValidCart_ReturnsOrder()
{
    // Arrange
    var mockCartService = new Mock<ICartService>();
    var mockOrderService = new Mock<IOrderService>();
    var facade = new OrderProcessingFacade(mockCartService.Object, ...);
    
    // Act
    var result = await facade.CreateAndProcessOrder(userId, request);
    
    // Assert
    Assert.True(result.Success);
    Assert.NotNull(result.Order);
    mockCartService.Verify(s => s.ClearCart(userId), Times.Once);
}
```

**Frontend (Vitest)**
```typescript
describe('PlantService', () => {
    let service: PlantService;
    let httpMock: HttpTestingController;
    
    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [PlantService]
        });
        service = TestBed.inject(PlantService);
        httpMock = TestBed.inject(HttpTestingController);
    });
    
    it('should fetch plants', (done) => {
        service.getPlants().subscribe(plants => {
            expect(plants.length).toBe(5);
            done();
        });
        
        const req = httpMock.expectOne('api/plants');
        req.flush([...]);
    });
});
```

### 9.3 E2E Testing (Cypress)

**Customer Flow**
```javascript
describe('Customer Shopping Flow', () => {
    it('should complete purchase with VNPay', () => {
        // Login
        cy.visit('/login');
        cy.get('[data-testid=email]').type('user@test.com');
        cy.get('[data-testid=password]').type('password');
        cy.get('[data-testid=login-btn]').click();
        
        // Browse products
        cy.get('[data-testid=plant-card]').first().click();
        cy.get('[data-testid=add-to-cart-btn]').click();
        
        // Checkout
        cy.get('[data-testid=cart-icon]').click();
        cy.get('[data-testid=checkout-btn]').click();
        cy.get('[data-testid=shipping-address]').type('123 Main St');
        cy.get('[data-testid=payment-vnpay]').click();
        cy.get('[data-testid=place-order-btn]').click();
        
        // Verify
        cy.url().should('include', '/order-confirmation');
        cy.get('[data-testid=order-number]').should('be.visible');
    });
});
```

**Admin Flow**
```javascript
describe('Admin Dashboard', () => {
    it('should manage products', () => {
        // Login as admin
        cy.adminLogin();
        
        // Create product
        cy.visit('/admin/products');
        cy.get('[data-testid=add-product-btn]').click();
        cy.get('[data-testid=product-name]').type('New Plant');
        cy.get('[data-testid=product-price]').type('150000');
        cy.get('[data-testid=save-btn]').click();
        
        // Verify
        cy.get('[data-testid=success-message]').should('be.visible');
        cy.get('[data-testid=product-row]').should('contain', 'New Plant');
    });
});
```

### 9.4 Performance Testing (K6)

**Load Test**
```javascript
import http from 'k6/http';
import { check } from 'k6';

export let options = {
    stages: [
        { duration: '1m', target: 100 },  // Ramp up
        { duration: '5m', target: 100 },  // Stay
        { duration: '1m', target: 0 }     // Ramp down
    ]
};

export default function() {
    let res = http.get('https://api.digixanh.com/api/plants');
    check(res, {
        'status is 200': r => r.status === 200,
        'response time < 200ms': r => r.timings.duration < 200
    });
}
```

**Checkout Flow Stress Test**
```javascript
export let options = {
    vus: 50,
    duration: '5m',
    thresholds: {
        http_req_duration: ['p(95)<500'],
        http_req_failed: ['rate<0.1']
    }
};
```

### 9.5 Test Coverage Goals

| Layer | Target | Current |
|-------|--------|---------|
| **Backend** | > 90% | 85% |
| **Frontend** | > 80% | 70% |
| **Controllers** | 100% | 100% |
| **Services** | > 95% | 93% |
| **DTOs** | > 80% | 85% |

### 9.6 Test Execution

```bash
# Frontend testing
npm run test:ci              # Unit tests
npm run e2e:ci              # E2E tests
npm run test:coverage       # Coverage report
npm run lint                # ESLint

# Backend testing (via CI/CD)
dotnet test /p:CollectCoverageRatio=true

# Performance testing
npm run perf:test           # Load test
npm run perf:checkout       # Checkout flow
npm run perf:stress         # Stress test
```

---

## 10. DEPLOYMENT

### 10.1 Deployment Architecture

```
┌─────────────────────────────────────┐
│    GitHub Repository (Source)       │
│    ├─ digixanh-fe/                  │
│    ├─ DigiXanh.API/                 │
│    └─ .github/workflows/            │
└──────────────┬──────────────────────┘
               │
               ↓ git push
┌──────────────────────────────────────────┐
│    GitHub Actions (CI/CD)                │
│    ├─ Run Tests                          │
│    ├─ Build Docker Image                 │
│    ├─ Push to Registry                   │
│    └─ Trigger Deployment                 │
└──────────────┬───────────────────────────┘
               │
        ┌──────┴──────┐
        ↓             ↓
┌──────────────────┐  ┌─────────────────────────┐
│ Frontend Deploy  │  │  Backend Deploy         │
│ (GitHub Pages)   │  │  (Render/Docker)        │
│                  │  │  (SQL Server Database)  │
└──────────────────┘  └─────────────────────────┘
```

### 10.2 Frontend Deployment (GitHub Pages)

**Build & Deploy Steps:**
```bash
# 1. Build production bundle
ng build --configuration=production --base-href=/DigiXanh/

# 2. Copy 404.html for SPA routing
cp dist/.../index.html dist/.../404.html

# 3. Deploy to GitHub Pages
angular-cli-ghpages --dir=dist/...
```

**Environment Configuration:**
- **Development**: `http://localhost:4200` + local backend
- **Production**: `https://tienxdun.github.io/DigiXanh/` + production API

### 10.3 Backend Deployment (Render)

**Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DigiXanh.API.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "DigiXanh.API.dll"]
```

**Render Deployment:**
- **Service**: Web Service (Docker)
- **Build Command**: `dotnet publish -c Release`
- **Start Command**: `dotnet DigiXanh.API.dll`
- **Environment Variables**: 
  - `ConnectionStrings__DefaultConnection`
  - `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`
  - `ASPNETCORE_ENVIRONMENT=Production`

### 10.4 Database Deployment

**Migration Strategy:**
```csharp
// Auto-migrate on startup
public class Program
{
    public static async Task Main(string[] args)
    {
        var app = builder.Build();
        
        // Ensure database is up-to-date
        await EnsureDatabaseReadyAsync(app.Services);
        
        app.Run();
    }
    
    static async Task EnsureDatabaseReadyAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        
        // Apply pending migrations
        await dbContext.Database.MigrateAsync();
        
        // Seed default data
        await SeedIdentityDataAsync(scope.ServiceProvider);
    }
}
```

### 10.5 CI/CD Pipeline (GitHub Actions)

**`.github/workflows/deploy.yml`:**
```yaml
name: Build and Deploy

on:
  push:
    branches: [main, develop]

jobs:
  frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '22'
      - name: Install dependencies
        run: cd digixanh-fe && npm ci
      - name: Run tests
        run: cd digixanh-fe && npm run test:ci
      - name: Build
        run: cd digixanh-fe && npm run build
      - name: Deploy to GitHub Pages
        uses: peaceiris/actions-gh-pages@v3
        with:
          github_token: ${{ secrets.GITHUB_TOKEN }}
          publish_dir: ./digixanh-fe/dist

  backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Restore
        run: cd DigiXanh.API && dotnet restore
      - name: Test
        run: cd DigiXanh.API && dotnet test
      - name: Publish
        run: cd DigiXanh.API && dotnet publish -c Release
      - name: Push to Docker Registry
        run: |
          docker build -t digixanh-api:latest .
          docker push ${{ secrets.DOCKER_REGISTRY }}/digixanh-api:latest
      - name: Deploy to Render
        run: curl ${{ secrets.RENDER_DEPLOY_HOOK }}
```

### 10.6 Environment Configurations

**Development (`.env.dev`):**
```
API_URL=http://localhost:5000
AUTH_ENABLED=true
DEBUG_MODE=true
```

**Production (`.env.prod`):**
```
API_URL=https://api.digixanh.com
AUTH_ENABLED=true
DEBUG_MODE=false
CACHE_ENABLED=true
```

### 10.7 Monitoring & Logging

**Logging Setup:**
```csharp
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// Exclude sensitive data
builder.Services.AddLogging(config =>
{
    config.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
});
```

**Metrics to Monitor:**
- API response times
- Error rates (4xx, 5xx)
- Database query performance
- Order processing time
- Payment success rate
- User authentication failures

### 10.8 Rollback Strategy

```bash
# Frontend: Revert GitHub Pages deployment
git revert <commit-hash>
git push
npm run deploy  # Redeploy previous version

# Backend: Revert Render deployment
git revert <commit-hash>
git push  # Automatic redeploy via GitHub Actions
```

---

## 📊 SUMMARY

### Key Metrics
- **Codebase**: ~50KB TypeScript (Frontend) + ~100KB C# (Backend)
- **Database**: 8+ tables, 30+ migrations
- **API Endpoints**: 40+ REST endpoints
- **Test Coverage**: 80%+ combined
- **Performance**: < 200ms average response time
- **Availability**: 99.9% uptime target

### Development Team Roles
- **Fullstack Developer**: All layers
- **DevOps Engineer**: CI/CD, infrastructure
- **QA Engineer**: Testing automation
- **Business Analyst**: Requirements & documentation

### Maintenance & Support
- **Bug Fix SLA**: 24-48 hours
- **Feature Development**: 1-2 week sprints
- **Security Updates**: As needed
- **Performance Optimization**: Monthly reviews

---

**Document Version**: 1.0  
**Last Updated**: March 2, 2026  
**Next Review**: June 2, 2026

