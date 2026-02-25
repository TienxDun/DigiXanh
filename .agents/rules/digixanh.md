---
trigger: always_on
---

Dưới đây là **quy trình làm việc chuẩn** dành cho các Agent (FE và BE) khi nhận task từ Product Owner (PO). Hãy tuân thủ nghiêm ngặt để đảm bảo hiệu quả và đồng bộ.

---

## 🔁 Quy trình làm việc khi nhận task

### 1. Tiếp nhận task
- **Nhận task** từ PO (qua comment, issue, hoặc bảng Scrum).
- **Đọc kỹ mô tả** và **tiêu chí chấp nhận (Acceptance Criteria)**.
- Nếu có bất kỳ điểm nào chưa rõ, **hỏi lại PO ngay lập tức** (không tự suy diễn).

### 2. Đọc tài liệu bắt buộc
- **Luôn đọc** các file sau **trước khi bắt đầu coding**:

| File | Ai đọc? | Nội dung |
|------|---------|----------|
| **context.md** | Cả hai | Tổng quan dự án, user stories, database, patterns, style guide, quy trình làm việc local, DoD. |
| **frontend-instruction.md** | FE Agent | Công nghệ FE, cấu trúc thư mục, cài đặt, coding conventions, phối hợp BE. |
| **backend-instruction.md** | BE Agent | Công nghệ BE, kiến trúc, patterns, tích hợp Trefle/VNPay, API design. |

> **Ghi nhớ:** Đây là "kim chỉ nam" xuyên suốt dự án. Nếu có thay đổi, PO sẽ cập nhật và thông báo.

### 3. Phân tích và làm rõ
- **Xác định phạm vi** task có liên quan đến cả FE và BE không? (Ví dụ: tích hợp API)
- Nếu **có**, liên hệ ngay với Agent còn lại (qua chat hoặc comment) để **thống nhất**:
  - API endpoint, method, request/response format (DTO).
  - Các thông số, mã lỗi, xác thực.
  - Thời gian dự kiến hoàn thành.
- Nếu **không**, tự lên kế hoạch chi tiết các bước cần làm.

### 4. Quy ước Git hiện tại
- Agent **không tự tạo nhánh/commit/push/PR** trừ khi PO yêu cầu rõ ràng trong task.
- PO là người **tự quản lý toàn bộ Git workflow** (branch, commit, push, PR, merge).

### 5. Phát triển (Coding)
- Tuân thủ **coding conventions** đã nêu trong file instruction.
- Viết code **sạch, có chú thích** nếu cần.
- Sau khi làm xong, agent cần **báo cáo rõ các file đã sửa** và cách test để PO tự commit.
- Đối với BE: viết **unit test** cho logic quan trọng (tính giá, xử lý order, pattern).
- Đối với FE: đảm bảo **responsive**, dùng **async pipe**, tránh memory leak.

### 6. Kiểm thử trên local
- **Chạy thử** đầy đủ luồng chức năng.
- **Kiểm tra lỗi** console (FE) hoặc log (BE).
- Đảm bảo **tích hợp API** (nếu có) hoạt động đúng.
- Nếu task liên quan đến FE và BE, **cả hai cùng test phối hợp** (FE gọi API thật từ local BE).

### 7. Đảm bảo Definition of Done (DoD)
Trước khi bàn giao cho PO, hãy tự kiểm tra:

- [ ] Code chạy đúng chức năng theo yêu cầu.
- [ ] Đã viết unit test (nếu có) và tất cả đều xanh.
- [ ] Giao diện (FE) đúng thiết kế, responsive.
- [ ] Không còn `console.log` hay code thừa.
- [ ] Đã format code đúng chuẩn.
- [ ] Đã ghi rõ hướng dẫn để PO tự commit/push nếu cần.

### 8. Bàn giao kết quả cho PO
- Tóm tắt các thay đổi đã thực hiện.
- Liệt kê file đã sửa và trạng thái test local.
- Đề xuất bước tiếp theo để PO tự commit/push/PR (nếu muốn).

### 9. Nhận feedback và chỉnh sửa
- Theo dõi phản hồi từ PO.
- Nếu có yêu cầu thay đổi, agent tiếp tục cập nhật code local và báo cáo lại phần thay đổi.

### 10. Kết thúc task
- Cập nhật trạng thái task trên bảng Scrum (nếu PO yêu cầu).
- Sẵn sàng nhận task mới từ PO.

---

## 📌 Lưu ý quan trọng
- **Luôn đọc lại context.md và file instruction** trước mỗi task để không bị lệch hướng.
- **Giao tiếp thường xuyên** với Agent còn lại để đảm bảo đồng bộ.
- Nếu gặp vấn đề kỹ thuật chưa rõ, hỏi PO hoặc cùng Agent kia tìm giải pháp.
- Git workflow do PO quản lý; agent chỉ thao tác Git khi có yêu cầu rõ ràng.
- **Tôn trọng thời gian**: Hoàn thành task đúng hạn, nếu có rủi ro, báo PO sớm.

---

Chúc các Agent làm việc hiệu quả và cùng nhau xây dựng DigiXanh thành công! 🌱