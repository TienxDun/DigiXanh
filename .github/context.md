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
- **Tích hợp:** Perenual API (lấy dữ liệu cây), VNPay Sandbox
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

## 4. Product Backlog (User Stories)

### 4.1. Các User Stories ĐÃ HOÀN THÀNH ✅

| US | Tiêu đề | Mô tả | Acceptance Criteria |
|----|---------|-------|---------------------|
| **US01** | **Đăng ký tài khoản** | Là khách hàng, tôi muốn đăng ký tài khoản để mua hàng và theo dõi đơn hàng | • Form đăng ký với Email, Password, FullName<br>• Validate: Email đúng format, Password >= 6 ký tự<br>• Tự động gán role "User"<br>• Trả về JWT token sau đăng ký |
| **US02** | **Đăng nhập** | Là ngườI dùng, tôi muốn đăng nhập để truy cập chức năng riêng | • Đăng nhập bằng Email + Password<br>• Trả về JWT token với role (Admin/User)<br>• ThờI gian hết hạn token: 60 phút<br>• Thông báo lỗi rõ ràng |
| **US03** | **Xem danh sách cây (Public)** | Là khách hàng, tôi muốn xem danh sách cây để chọn mua | • Phân trang (mặc định 12 items/page)<br>• Tìm kiếm theo tên/tên khoa học<br>• Lọc theo danh mục<br>• Sắp xếp: Giá tăng/giảm, Tên A-Z, MớI nhất<br>• Hiển thị: Tên, giá, ảnh, danh mục |
| **US04** | **Xem chi tiết cây** | Là khách hàng, tôi muốn xem chi tiết cây để biết thông tin đầy đủ | • Hiển thị: Tên, tên khoa học, mô tả, giá, ảnh, tồn kho<br>• Trả 404 nếu cây không tồn tại hoặc đã xóa |
| **US05** | **Quản lý cây - Admin CRUD** | Là admin, tôi muốn quản lý danh sách cây | • Phân trang, tìm kiếm<br>• Thêm/Sửa/Xóa (soft delete) cây<br>• Validate CategoryId tồn tại<br>• Chỉ Admin mới có quyền |
| **US06** | **Soft Delete & Bulk Delete** | Là admin, tôi muốn xóa cây an toàn | • Soft delete (đánh dấu IsDeleted = true)<br>• Xóa nhiều cây cùng lúc (bulk)<br>• Cây đã xóa không hiển thị ở public API |
| **US07** | **Quản lý danh mục** | Là admin, tôi muốn quản lý danh mục cây | • API CRUD danh mục<br>• Danh mục có thể phân cấp (ParentCategoryId)<br>• Danh sách danh mục cho public và admin |
| **US08** | **Dashboard thống kê** | Là admin, tôi muốn xem thống kê để quản lý | • Tổng số đơn hàng<br>• Tổng doanh thu (Paid + Delivered)<br>• Đơn hàng hôm nay<br>• Doanh thu hôm nay<br>• Biểu đồ đơn hàng 7 ngày gần nhất |
| **US09** | **Tích hợp Perenual API** | Là admin, tôi muốn tìm cây từ API bên ngoài để thêm nhanh | • Tìm kiếm cây theo tên qua Perenual API<br>• Xem chi tiết cây từ Perenual<br>• Handle timeout và rate limit<br>• Tự động điền form thêm cây |
| **US10** | **Giỏ hàng** | Là khách hàng, tôi muốn quản lý giỏ hàng | • Thêm sản phẩm vào giỏ<br>• Cập nhật số lượng (1-99)<br>• Xóa sản phẩm khỏi giỏ<br>• Xem tổng quan giỏ hàng<br>• Validate tồn kho khi thêm |
| **US11** | **Đặt hàng (Checkout)** | Là khách hàng, tôi muốn đặt hàng từ giỏ hàng | • Nhập: Tên ngườI nhận, SĐT, Địa chỉ giao hàng<br>• Chọn phương thức thanh toán (Cash/VNPay)<br>• Tính giá với Decorator Pattern (giảm 5% nếu >=2sp, 7% nếu >=3sp)<br>• Validate tồn kho trước khi đặt<br>• Xóa giỏ hàng sau đặt thành công |
| **US12** | **Thanh toán VNPay** | Là khách hàng, tôi muốn thanh toán online qua VNPay | • Tạo URL thanh toán VNPay Sandbox<br>• Xử lý Return URL (success/failed)<br>• Xử lý IPN (Instant Payment Notification)<br>• Khôi phục giỏ hàng nếu thanh toán thất bại<br>• Validate signature |
| **US13** | **Quản lý đơn hàng - Admin** | Là admin, tôi muốn quản lý đơn hàng | • Danh sách đơn hàng (phân trang, lọc theo status, tìm kiếm)<br>• Chi tiết đơn hàng (items, lịch sử trạng thái)<br>• Cập nhật trạng thái đơn hàng với validation transition<br>• Audit log (ai thay đổi, lý do) |
| **US14** | **Lịch sử trạng thái đơn hàng** | Là admin, tôi muốn theo dõi lịch sử thay đổi đơn hàng | • Tự động ghi log khi thay đổi trạng thái<br>• Lưu: OldStatus, NewStatus, ChangedBy, Reason, Timestamp<br>• Hiển thị lịch sử trong chi tiết đơn hàng |
| **US15** | **Adapter Pattern - Thanh toán** | Hệ thống cần hỗ trợ nhiều phương thức thanh toán | • Interface: `IPaymentAdapter`<br>• Implementations: `CashPaymentAdapter`, `VNPayPaymentAdapter`<br>• Factory pattern để chọn adapter theo PaymentMethod |
| **US16** | **Decorator Pattern - Tính giá** | Hệ thống cần tính giá linh hoạt với giảm giá | • Interface: `IPriceCalculator`<br>• `BasePriceCalculator`: Tính tổng cơ bản<br>• `QuantityDiscountDecorator`: Giảm 5% (>=2sp), 7% (>=3sp)<br>• Có thể mở rộng thêm decorator khác |
| **US17** | **Facade Pattern - Xử lý đơn hàng** | Đơn giản hóa quy trình đặt hàng phức tạp | • `OrderProcessingFacade` đóng gói toàn bộ flow<br>• Validate → Tính giá → Tạo Order → Thanh toán → Email → Xóa giỏ<br>• Transaction rollback nếu có lỗi<br>• Xử lý VNPay callback trong facade |
| **US26** | **Quản lý tồn kho** | Hệ thống cần quản lý số lượng tồn kho sản phẩm | • Thêm trường StockQuantity cho Plant<br>• Validate tồn kho khi thêm vào giỏ<br>• Validate tồn kho khi đặt hàng<br>• Tự động giảm tồn kho sau thanh toán thành công<br>• Hiển thị trạng thái "Hết hàng" nếu Stock = 0 |

---

### 4.2. Các User Stories CẦN LÀM (Backlog) 📋

#### 🔴 **Mức độ ưu tiên CAO (High Priority)**

| US | Tiêu đề | Mô tả | Acceptance Criteria | Est. |
|----|---------|-------|---------------------|------|
| **US18** | **Xem lịch sử đơn hàng cá nhân** | Là khách hàng, tôi muốn xem các đơn hàng đã đặt | • API lấy danh sách đơn hàng của user hiện tại<br>• Phân trang, sắp xếp theo thờI gian mới nhất<br>• Hiển thị: Mã đơn, ngày đặt, tổng tiền, trạng thái<br>• FE: Trang "Đơn hàng của tôi" | 3SP |
| **US19** | **Chi tiết đơn hàng cá nhân** | Là khách hàng, tôi muốn xem chi tiết đơn hàng | • API chi tiết đơn hàng (chỉ của user hiện tại)<br>• Hiển thị: Thông tin đơn, danh sách items, trạng thái<br>• FE: Trang chi tiết đơn hàng | 2SP |
| **US20** | **Hủy đơn hàng (Customer)** | Là khách hàng, tôi muốn hủy đơn hàng chưa xử lý | • Chỉ cho phép hủy đơn ở trạng thái Pending<br>• Khôi phục tồn kho khi hủy<br>• Ghi log lịch sử hủy<br>• Gửi thông báo xác nhận hủy | 2SP |
| **US21** | **Cập nhật thông tin cá nhân** | Là ngườI dùng, tôi muốn cập nhật thông tin cá nhân | • API cập nhật: FullName, Phone, Address<br>• Validate SĐT Việt Nam<br>• FE: Trang Profile/Settings<br>• Không cho đổi email | 2SP |

#### 🟡 **Mức độ ưu tiên TRUNG BÌNH (Medium Priority)**

| US | Tiêu đề | Mô tả | Acceptance Criteria | Est. |
|----|---------|-------|---------------------|------|
| **US22** | **Trang chủ (Homepage)** | Là khách hàng, tôi muốn xem trang chủ đẹp | • Hero banner/slider<br>• Hiển thị sản phẩm nổi bật (featured)<br>• Danh mục phổ biến<br>• Footer với thông tin liên hệ | 3SP |
| **US23** | **Tìm kiếm nâng cao** | Là khách hàng, tôi muốn tìm kiếm nâng cao | • Tìm theo khoảng giá (min-max)<br>• Tìm theo nhiều danh mục cùng lúc<br>• Sắp xếp đa dạng (giá, tên, mớI nhất)<br>• Gợi ý tìm kiếm (autocomplete) | 3SP |
| **US24** | **Đánh giá sản phẩm** | Là khách hàng, tôi muốn đánh giá sản phẩm đã mua | • Chỉ đánh giá sản phẩm trong đơn Delivered<br>• Rating 1-5 sao + comment<br>• Hiển thị đánh giá ở trang chi tiết sản phẩm<br>• Average rating cho mỗi sản phẩm | 3SP |
| **US25** | **Yêu thích sản phẩm** | Là khách hàng, tôi muốn lưu sản phẩm yêu thích | • Thêm/Xóa khỏi danh sách yêu thích<br>• Xem danh sách yêu thích của tôi<br>• Persist vào database (bảng Favorites) | 2SP |
| **US27** | **Quản lý ngườI dùng - Admin** | Là admin, tôi muốn quản lý tài khoản ngườI dùng | • Danh sách users (phân trang, tìm kiếm)<br>• Xem chi tiết user<br>• Khóa/Mở khóa tài khoản<br>• Phân quyền (đổi role) | 3SP |
| **US28** | **Thống kê nâng cao - Admin** | Là admin, tôi muốn xem báo cáo chi tiết | • Doanh thu theo tháng/quý/năm<br>• Top sản phẩm bán chạy<br>• Biểu đồ doanh thu theo danh mục<br>• Export báo cáo (CSV/Excel) | 3SP |
| **US29** | **Quản lý Banner/Slider** | Là admin, tôi muốn quản lý banner trang chủ | • CRUD banner (tiêu đề, ảnh, link, thứ tự)<br>• Kích hoạt/Vô hiệu hóa banner<br>• Hiển thị ở trang chủ FE | 2SP |

#### 🟢 **Mức độ ưu tiên THẤP (Low Priority) / Nice to have**

| US | Tiêu đề | Mô tả | Acceptance Criteria | Est. |
|----|---------|-------|---------------------|------|
| **US30** | **Gửi Email thực tế** | Hệ thống cần gửi email thực tế cho ngườI dùng | • Tích hợp SMTP (Gmail/SendGrid)<br>• Email đặt hàng thành công<br>• Email thanh toán thành công<br>• Email cập nhật trạng thái đơn hàng<br>• Template email đẹp | 3SP |
| **US31** | **Nhập hàng/Restock - Admin** | Là admin, tôi muốn quản lý nhập hàng | • Form nhập hàng: Chọn sản phẩm, số lượng, giá nhập<br>• Cập nhật tồn kho khi nhập<br>• Lịch sử nhập hàng<br>• Báo cáo tồn kho thấp (alert) | 3SP |
| **US32** | **Mã giảm giá (Coupon)** | Là khách hàng, tôi muốn áp dụng mã giảm giá | • CRUD mã giảm giá (Admin)<br>• Áp dụng mã ở checkout: % giảm hoặc fixed amount<br>• Validate: Hạn sử dụng, số lần dùng, đơn hàng tối thiểu<br>• Hiển thị giá sau giảm | 3SP |
| **US33** | **Danh sách yêu thích đồng bộ** | Là khách hàng, tôi muốn lưu yêu thích vào tài khoản | • Đồng bộ wishlist khi đăng nhập trên thiết bị khác<br>• Merge wishlist local với server<br>• Hiển thị indicator số lượng wishlist | 2SP |
| **US34** | **So sánh sản phẩm** | Là khách hàng, tôi muốn so sánh nhiều sản phẩm | • Chọn tối đa 3 sản phẩm để so sánh<br>• Bảng so sánh: Giá, danh mục, tên khoa học<br>• Modal hoặc trang riêng | 2SP |
| **US35** | **Gợi ý sản phẩm liên quan** | Là khách hàng, tôi muốn xem sản phẩm tương tự | • Gợi ý cùng danh mục<br>• Gợi ý dựa trên lịch sử xem (nếu có)<br>• Hiển thị ở trang chi tiết sản phẩm | 2SP |
| **US36** | **Đa ngôn ngữ (i18n)** | Hệ thống hỗ trợ đa ngôn ngữ | • Hỗ trợ Tiếng Việt và English<br>• Switch language ở header<br>• Lưu preference vào localStorage | 3SP |
| **US37** | **Dark Mode** | Là ngườI dùng, tôi muốn dùng giao diện tối | • Toggle dark/light mode<br>• Lưu preference<br>• CoreUI đã hỗ trợ, chỉ cần implement toggle | 1SP |
| **US38** | **Chat/Chatbot hỗ trợ** | Là khách hàng, tôi muốn được hỗ trợ trực tuyến | • Widget chat ở góc phải<br>• Chat với admin (real-time hoặc polling)<br>• Hoặc tích hợp chatbot cơ bản | 5SP |
| **US39** | **Tích hợp Firebase Cloud Messaging** | Gửi thông báo push cho ngườI dùng | • Thông báo khi đơn hàng thay đổi trạng thái<br>• Thông báo khuyến mãi<br>• Subscribe/unsubscribe topics | 3SP |
| **US40** | **SEO Optimization** | Cải thiện SEO cho website | • Meta tags động theo trang<br>• Sitemap.xml<br>• Robots.txt<br>• Structured data (JSON-LD) | 2SP |

---

### 4.3. Bảng tổng hợp tiến độ

| Phân loại | Số lượng | Tình trạng |
|-----------|----------|------------|
| **Đã hoàn thành** | 18 US | ✅ Các chức năng core của MVP |
| **Ưu tiên Cao** | 4 US | 🔴 Cần làm ngay sau khi MVP hoàn thành |
| **Ưu tiên Trung bình** | 7 US | 🟡 Làm trong Sprint tiếp theo |
| **Ưu tiên Thấp** | 11 US | 🟢 Nice to have, làm khi có thờI gian |
| **Tổng cộng** | 40 US | |

**Ghi chú về Sprint Planning:**
- **MVP (Sprint 1-2):** Hoàn thành 18 US (US01-US17, US26) - ĐÃ XONG
- **Sprint 3:** US18-US21 (Lịch sử đơn hàng, Profile) - 🔴 Ưu tiên cao
- **Sprint 4:** US22-US25, US27-US29 (Nâng cao UX, Admin features) - 🟡 Trung bình
- **Sprint 5+:** US30-US40 (Email, Coupon, i18n, etc.) - 🟢 Thấp

---

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
4. `20260225165902_AddPlantDescriptionAndTrefleId` - Thêm mô tả (TrefleId đã bị xóa)
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

## 7. Tích hợp Perenual API
- **Tài liệu:** [https://perenual.com/docs/api](https://perenual.com/docs/api)
- **API Key:** Cần đăng ký tài khoản Perenual để lấy key.
- **Endpoints chính:**
  - `GET /api/species-list?q={query}` – tìm kiếm cây theo tên
  - `GET /api/species/details/{id}` – lấy chi tiết cây
- **Cách dùng:** Admin nhập tên cây → gọi `search` → hiển thị danh sách → chọn → gọi `details` → tự động điền form thêm cây. Chỉ lưu vào DB sau khi admin nhập giá và danh mục.
- **Xử lý rate limit:** Perenual giới hạn số request (100/ngày), nên cache kết quả tìm kiếm trong phiên làm việc (session) hoặc dùng memory cache ngắn hạn.

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
  "Perenual": {
    "ApiKey": "your-perenual-api-key"
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
