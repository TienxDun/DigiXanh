# 📱 DigiXanh - Tài liệu Trang UI

> File này tổng hợp tất cả các trang/UI hiện có trong dự án để FE tham khảo khi phát triển.

---

## 📂 Cấu trúc thư mục Views

```
src/app/views/
├── admin/              # Admin management pages
├── auth/               # Authentication pages
├── cart/               # Shopping cart & checkout
├── dashboard/          # Admin dashboard
├── pages/              # Static pages (403, etc.)
├── plants/             # Plant related (legacy)
└── public/             # Public site pages
```

---

## 🌐 PUBLIC PAGES (Không cần đăng nhập)

### Layout: `PublicLayoutComponent`

| Route | Component | Tên trang | Mô tả |
|-------|-----------|-----------|-------|
| `/` | `PublicPlantListComponent` | Danh sách cây | Trang chủ hiển thị danh sách cây, tìm kiếm, lọc |
| `/plants/:id` | `PublicPlantDetailComponent` | Chi tiết cây | Xem chi tiết cây, thêm vào giỏ hàng |

### Layout: Không có (Standalone)

| Route | Component | Tên trang | Mô tả |
|-------|-----------|-----------|-------|
| `/auth/login` | `LoginComponent` | Đăng nhập | Form đăng nhập |
| `/auth/register` | `RegisterComponent` | Đăng ký | Form đăng ký tài khoản |
| `/auth` | redirect → `/auth/login` | - | Tự động chuyển đến login |
| `/403` | `Page403Component` | Truy cập bị từ chối | Hiển thị khi không có quyền |

---

## 🛒 USER PAGES (Cần đăng nhập - `authGuard`)

### Layout: `PublicLayoutComponent`

| Route | Component | Tên trang | Mô tả |
|-------|-----------|-----------|-------|
| `/cart` | `CartComponent` | Giỏ hàng | Xem/sửa số lượng/xóa sản phẩm trong giỏ |
| `/checkout` | `CheckoutComponent` | Thanh toán | Form nhập thông tin giao hàng, chọn phương thức thanh toán |
| `/order-success` | `OrderSuccessComponent` | Đặt hàng thành công | Hiển thị sau khi đặt hàng thành công |
| `/payment-return` | `PaymentReturnComponent` | Kết quả thanh toán | Xử lý callback từ VNPay sau thanh toán |

---

## 🔐 ADMIN PAGES (Cần Admin role - `adminGuard`)

### Layout: `DefaultLayoutComponent` (CoreUI Sidebar)

| Route | Component | Menu Sidebar | Mô tả |
|-------|-----------|--------------|-------|
| `/admin` | redirect → `/admin/dashboard` | - | Trang mặc định admin |
| `/admin/dashboard` | `DashboardComponent` | ✅ Dashboard | Thống kê tổng quan (đơn hàng, doanh thu) |
| `/admin/plants` | `PlantListComponent` | ✅ Quản lý cây | Danh sách cây (phân trang, tìm kiếm, xóa) |
| `/admin/plants/create` | `AddPlantComponent` | - | Thêm cây mới |
| `/admin/plants/edit/:id` | `AddPlantComponent` | - | Chỉnh sửa cây (reuse component create) |
| `/admin/orders` | `OrderListComponent` | ✅ Quản lý đơn hàng | Danh sách đơn hàng (phân trang, lọc, tìm kiếm) |
| `/admin/orders/:id` | `OrderDetailComponent` | - | Chi tiết đơn hàng + cập nhật trạng thái |

---

## 📊 Chi tiết Components

### 1. Public Pages

#### `PublicPlantListComponent`
- **Path**: `src/app/views/public/public-plant-list/`
- **Features**: 
  - Hiển thị grid danh sách cây
  - Tìm kiếm theo tên
  - Lọc theo danh mục
  - Phân trang
  - Thêm vào giỏ hàng

#### `PublicPlantDetailComponent`
- **Path**: `src/app/views/public/public-plant-detail/`
- **Features**:
  - Thông tin chi tiết cây
  - Hình ảnh sản phẩm
  - Chọn số lượng
  - Thêm vào giỏ hàng
  - Hiển thị cây liên quan

### 2. Auth Pages

#### `LoginComponent`
- **Path**: `src/app/views/auth/login/`
- **Features**: Form đăng nhập, link đến register

#### `RegisterComponent`
- **Path**: `src/app/views/auth/register/`
- **Features**: Form đăng ký, validation

### 3. Cart Pages

#### `CartComponent`
- **Path**: `src/app/views/cart/cart.component.ts`
- **Features**:
  - Danh sách sản phẩm trong giỏ
  - Thay đổi số lượng
  - Xóa sản phẩm
  - Tính tổng tiền
  - Button chuyển đến checkout

#### `CheckoutComponent`
- **Path**: `src/app/views/cart/checkout.component.ts`
- **Features**:
  - Form thông tin giao hàng
  - Chọn phương thức thanh toán (Cash/VNPay)
  - Hiển thị giảm giá theo số lượng
  - Xác nhận đặt hàng

### 4. Admin Pages

#### `DashboardComponent`
- **Path**: `src/app/views/dashboard/dashboard.component.ts`
- **Features**:
  - Thống kê tổng đơn hàng
  - Doanh thu
  - Biểu đồ đơn hàng 7 ngày gần nhất

#### `PlantListComponent`
- **Path**: `src/app/views/admin/plants/plant-list/`
- **Features**:
  - Bảng danh sách cây
  - Tìm kiếm
  - Phân trang
  - Checkbox chọn nhiều
  - Xóa đơn/xóa hàng loạt
  - Link edit/create

#### `AddPlantComponent` (Create/Edit)
- **Path**: `src/app/views/admin/plants/add-plant/`
- **Features**:
  - Form thêm/sửa cây
  - Tích hợp Trefle API search
  - Upload/URL hình ảnh
  - Chọn danh mục

#### `OrderListComponent` 🆕
- **Path**: `src/app/views/admin/orders/order-list/`
- **Features**:
  - Bảng đơn hàng
  - Tìm kiếm (ID, tên KH, email, SĐT)
  - Lọc theo trạng thái
  - Phân trang
  - Badge trạng thái màu sắc

#### `OrderDetailComponent` 🆕
- **Path**: `src/app/views/admin/orders/order-detail/`
- **Features**:
  - Thông tin khách hàng
  - Địa chỉ giao hàng
  - Danh sách sản phẩm
  - Lịch sử trạng thái (audit trail)
  - Cập nhật trạng thái đơn hàng

---

## 🎨 Layouts

### 1. `PublicLayoutComponent`
- **Path**: `src/app/layout/public-layout/`
- **Dùng cho**: Public pages (trang chủ, chi tiết cây)
- **Features**: Header, Footer, Navigation công khai

### 2. `DefaultLayoutComponent` (CoreUI)
- **Path**: `src/app/layout/default-layout/`
- **Dùng cho**: Admin pages
- **Features**:
  - Sidebar navigation (navItems)
  - Header
  - Breadcrumb
  - Responsive design

---

## 📝 Menu Sidebar (Admin)

File: `src/app/layout/default-layout/_nav.ts`

```typescript
export const navItems: INavData[] = [
  { title: true, name: 'Tổng quan' },
  { name: 'Dashboard', url: '/admin/dashboard', icon: 'cil-speedometer' },
  { title: true, name: 'Danh mục quản lý' },
  { name: 'Quản lý cây', url: '/admin/plants', icon: 'cil-leaf' },
  { name: 'Quản lý đơn hàng', url: '/admin/orders', icon: 'cil-cart' }
];
```

---

## 🔄 Luồng chuyển trang chính

### User Flow:
```
Home (/) 
  → Plant Detail (/plants/:id) 
  → Add to Cart 
  → Cart (/cart) [login required]
  → Checkout (/checkout)
  → Order Success (/order-success)
```

### Admin Flow:
```
Login (/auth/login)
  → Dashboard (/admin/dashboard)
    → Plants (/admin/plants)
      → Create/Edit
    → Orders (/admin/orders)
      → Order Detail (/admin/orders/:id)
```

---

## 🔌 API Services tương ứng

| Service | File | Chức năng |
|---------|------|-----------|
| `PlantService` | `core/services/plant.service.ts` | Lấy danh sách cây, chi tiết cây |
| `AdminPlantService` | `core/services/admin-plant.service.ts` | CRUD cây (admin) |
| `CartService` | `core/services/cart.service.ts` | Quản lý giỏ hàng |
| `OrderService` | `core/services/order.service.ts` | Đặt hàng, quản lý đơn (admin) |
| `AuthService` | `core/services/auth.service.ts` | Đăng nhập/đăng ký |
| `DashboardService` | `core/services/dashboard.service.ts` | Thống kê |

---

## 🛡️ Guards

| Guard | File | Dùng cho |
|-------|------|----------|
| `authGuard` | `core/guards/auth.guard.ts` | Các trang cần đăng nhập (cart, checkout) |
| `adminGuard` | `core/guards/admin.guard.ts` | Các trang admin (/admin/*) |

---

## 🎯 US hiện tại đã implement

- ✅ US01-US03: Auth (Login/Register/Phân quyền)
- ✅ US04-US07: Admin CRUD Plants + Dashboard
- ✅ US08-US10: Public xem cây + Giỏ hàng
- ✅ US11-US15: Đặt hàng + Thanh toán (Cash/VNPay) + Design Patterns
- ✅ US19-US21: 🆕 Quản lý đơn hàng (User & Admin)

---

## 📝 Chú ý cho FE Developer

1. **Icons**: Dùng Font Awesome (cil-* cho CoreUI, fa-* cho Font Awesome)
2. **Style**: CoreUI Bootstrap 5 + SCSS custom
3. **Responsive**: Mobile-first approach
4. **Loading**: Sử dụng BehaviorSubject pattern với `loading$`
5. **Error handling**: Hiển thị alert/message khi có lỗi

---

*Cập nhật: 2026-02-28*
