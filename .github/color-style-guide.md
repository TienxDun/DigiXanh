# 🌱 DigiXanh – Premium Color Style Guide

Tài liệu này định nghĩa hệ thống màu sắc chuẩn mực mang phong cách **"Premium Glassmorphism & Pastel"** được áp dụng riêng cho dự án DigiXanh. 
Tất cả các tính năng mới khi phát triển phải tuân thủ nghiêm ngặt bảng màu này để giữ được tính đồng nhất, cao cấp và trải nghiệm xuyên suốt cho khách hàng lẫn quản trị viên.

---

## 1. Triết lý thiết kế (Design Philosophy)
- Dịu mắt, thân thiện với thiên nhiên (Nature-friendly).
- Premium: Không sử dụng màu nhồi (solid / vivid) đậm đặc gây chói mắt.
- Pastel / Glassmorphism: Tất cả các Badge, nhãn dán, trạng thái phải dùng nền mờ `bg-opacity-10`, đi kèm với viền nhạt `border-opacity-25` và chữ cùng tone màu.

---

## 2. Hệ thống màu Trạng Thái (Status Colors)

Là các màu được sử dụng cho cả Khách hàng và Admin khi hiển thị trạng thái Đơn hàng, Trạng thái thanh toán, Trạng thái sản phẩm.

| Trạng thái (Status) | Mã Bootstrap 5 Classes | Ý nghĩa / Áp dụng |
| ------------------- | -------------------------------------------------------------------------------------------------- | ------------------------------------------------------ |
| **Pending** (Chờ xử lý) | `bg-warning bg-opacity-10 text-warning border border-warning border-opacity-25` | Chờ xác nhận, Chờ thanh toán, Trạng thái tạm thời. |
| **Paid** (Đã thanh toán) | `bg-info bg-opacity-10 text-info border border-info border-opacity-25` | Đơn đã thanh toán Điện tử, Đang chờ giao hàng đi. |
| **Shipped** (Đang giao) | `bg-primary bg-opacity-10 text-primary border border-primary border-opacity-25` | Giao hàng, Vận chuyển, Quá trình đang diễn ra. |
| **Delivered** (Đã giao) | `bg-success bg-opacity-10 text-success border border-success border-opacity-25` | Hoàn thành, Thành công, Đã nhận hàng. |
| **Cancelled** (Đã huỷ) | `bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25` | Bị hủy, Lỗi, Thất bại, Cảnh báo đỏ. |
| **Default** (Mặc định) | `bg-secondary bg-opacity-10 text-secondary border border-secondary border-opacity-25` | Tiền mặt, Chưa xác định, Trạng thái nháp hoặc chờ. |

---

## 3. Hệ thống màu Thanh Toán (Payment Methods)

Được chuẩn hoá để phân biệt nhanh phương thức thanh toán trong danh sách.

- **Tiền Mặt (COD)**: 
  Sử dụng chuẩn màu Default/Xám bạc. Mang lại cảm giác truyền thống, tĩnh lặng.
  `bg-secondary bg-opacity-10 text-secondary border border-secondary border-opacity-25`

- **VNPay / Online**: 
  Sử dụng chuẩn màu Success/Xanh lá. Mang lại cảm giác an toàn, công nghệ và đã xác thực.
  `bg-success bg-opacity-10 text-success border border-success border-opacity-25`

---

## 4. Typography (Màu sắc chữ viết)

Quy tắc đặt màu chữ để không bị đen gắt (Mã #000) hay xám mờ không đọc được:

- **Tiêu đề (Headings, Titles)**: `text-dark` (Màu đen dịu của Bootstrap, tạo độ tương phản tốt, không bị sắc gắt).
- **Văn bản chính (Body, Descriptions)**: `text-muted` hoặc `text-secondary` nếu chữ cần có sự nhẹ nhàng, không chiếm Spotlight.
- **Giá tiền / Điểm nhấn**: `text-primary` (Xanh lá - màu thương hiệu DigiXanh) đi kèm với `fw-bold`.
- **Tổng tiền thanh toán**: Tuỳ context, thưòng dùng `text-success` (nếu là kết quả tốt) hoặc `text-primary` đi kèm size chữ to `fs-4`, `fs-5`.

---

## 5. Hướng dẫn sử dụng mẫu (Angular Code Snippet)

Khi tạo ra Component mới (ví dụ: Trang chi tiết khách hàng, Trang quản lý kho, v.v.), hãy tạo một hàm trả về class Bootstrap như sau vào trong file `.ts`:

```typescript
  getBootstrapBadgeClass(type: string): string {
    switch (type.toLowerCase()) {
      case 'warning':
      case 'pending':
        return 'bg-warning bg-opacity-10 text-warning border border-warning border-opacity-25';
      case 'success':
      case 'delivered':
      case 'vnpay':
        return 'bg-success bg-opacity-10 text-success border border-success border-opacity-25';
      case 'error':
      case 'cancelled':
        return 'bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25';
      case 'info':
      case 'paid':
        return 'bg-info bg-opacity-10 text-info border border-info border-opacity-25';
      case 'primary':
      case 'shipped':
        return 'bg-primary bg-opacity-10 text-primary border border-primary border-opacity-25';
      default:
        // Cash, Draft, Default
        return 'bg-secondary bg-opacity-10 text-secondary border border-secondary border-opacity-25';
    }
  }
```

Và sử dụng tại HTML (Lưu ý thêm các Helper class tạo hình):
```html
<span class="badge rounded-pill px-3 py-1 fw-medium" [ngClass]="getBootstrapBadgeClass(item.status)">
  {{ item.statusDisplay | uppercase }}
</span>
```

---
**Chốt lại:** Hãy luôn nhớ từ khóa **"Sạch sẽ, Pastel, Mềm mại, Chuyên nghiệp"** trong mỗi thiết kế mới của dự án. Không sử dụng các Class `.bg-danger`, `.bg-success` solid thuần tuý trừ trường hợp với các Nút (Button) cần sự chú ý lớn từ người dùng (Call-to-Action).
