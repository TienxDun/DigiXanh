# Kế hoạch tối ưu hiệu năng FE

## 1) Phạm vi và mục tiêu
- Phạm vi: Ứng dụng frontend trong `digixanh-fe` (public pages + auth + cart/checkout + admin).
- Mục tiêu chính:
  - Cải thiện độ mượt khi cuộn trang và điều hướng.
  - Giảm chi phí tải ban đầu ở các route quan trọng.
  - Đảm bảo hành vi vẫn ổn định sau mỗi bước tối ưu.
- KPI đề xuất:
  - LCP < 2.5s (mobile, p75)
  - INP < 200ms (mobile, p75)
  - CLS < 0.1
  - Giới hạn bundle JS ban đầu theo route
  - 55-60 FPS cho tương tác cuộn thông thường trên thiết bị mục tiêu

## 2) Đo baseline và rào chắn an toàn (bắt buộc làm trước)
### Công việc
- Ghi nhận baseline trước khi sửa code:
  - Lighthouse (mobile + desktop) cho các route quan trọng
  - Web Vitals trong trình duyệt (LCP/INP/CLS)
  - Chrome Performance trace cho các trang cuộn nhiều
  - Snapshot kích thước bundle từ production build

### Quality Gate (bắt buộc đạt trước khi qua bước tiếp theo)
- `npm run test:ci`
- `npm run build`
- `npm run e2e:ci` (tối thiểu các luồng smoke)
- Lưu artifact báo cáo baseline vào `docs/perf-baseline/` (hoặc thư mục đã chọn).

## 3) Danh sách tối ưu ưu tiên

### Giai đoạn A - Tác động cao, rủi ro thấp
1. Tách code theo route và lazy load
- Đảm bảo các module nặng được lazy load.
- Trì hoãn các khối UI không quan trọng ở bên dưới màn hình.

Quality gate sau bước này:
- Build thành công và không lỗi điều hướng route.
- So sánh JS payload ban đầu với baseline.
- `npm run test:ci` + e2e smoke.

2. Tối ưu hình ảnh và tài nguyên
- Chuyển ảnh lớn sang định dạng hiện đại (WebP/AVIF nếu phù hợp).
- Cung cấp kích thước ảnh responsive và lazy loading cho media dưới màn hình.
- Chỉ preload ảnh hero hoặc tài nguyên hình ảnh thực sự quan trọng.

Quality gate sau bước này:
- Không vỡ hình ảnh ở các route chính.
- LCP không tệ hơn baseline ở homepage/product detail.
- `npm run test:ci` + e2e smoke.

3. Giảm rerender không cần thiết / áp lực change detection
- Audit các component cập nhật thường xuyên (list, cart, dashboard widgets).
- Dùng `OnPush`, giá trị tính toán được nhớ hóa, và `trackBy` cho danh sách.

Quality gate sau bước này:
- Không phát sinh lỗi UI stale sau tương tác.
- Độ phản hồi scroll và input cải thiện ở các view bị ảnh hưởng.
- Unit test cho component thay đổi được cập nhật.

### Giai đoạn B - Độ mượt tương tác
4. Tối ưu sự kiện cuộn và event handler
- Throttle/debounce `scroll`, `resize`, và các handler tần suất cao.
- Dùng passive listeners cho scroll/touch khi phù hợp.
- Đưa tính toán nặng đồng bộ ra khỏi UI thread khi cần.

Quality gate sau bước này:
- Không hồi quy ở sticky header, infinite scroll, hoặc bộ lọc.
- Performance trace cho thấy giảm long task khi cuộn.
- `npm run test:ci` + e2e mục tiêu.

5. Giảm chi phí render DOM/layout và CSS
- Loại bỏ các mẫu gây layout thrashing.
- Ưu tiên animation bằng `transform` và `opacity`.
- Giảm hiệu ứng tốn tài nguyên (blur/shadow lớn) trên breakpoint mobile.

Quality gate sau bước này:
- Kiểm tra visual regression trên màn hình quan trọng.
- CLS giữ <= baseline.
- `npm run build` + e2e smoke.

### Giai đoạn C - Mạng và phân phối dữ liệu
6. Tối ưu lấy dữ liệu API
- Loại bỏ request trùng lặp, cache theo query key, tránh refetch không cần.
- Dùng pagination/infinite loading nếu full-list fetch quá nặng.
- Thêm prefetch cho dữ liệu route có khả năng truy cập tiếp theo.

Quality gate sau bước này:
- Không có lỗi dữ liệu stale trong luồng cart/order.
- Số request API giảm so với baseline trên các hành trình mục tiêu.
- `npm run test:ci` + e2e luồng checkout.

7. Chiến lược cache và phân phối
- Xác nhận cache headers cho static assets và content hashing.
- Đảm bảo chiến lược CDN/static serving phù hợp deployment.

Quality gate sau bước này:
- Tải lại lần 2 nhanh hơn cold load.
- Không có cache poisoning/stale asset mismatch.
- Production build và deployment smoke test đạt.

## 4) Ràng buộc cứng để giữ website ổn định
- Không merge thay đổi tối ưu nếu chưa đạt tất cả gate của bước đó.
- Mỗi PR chỉ tập trung một nhóm tối ưu (tránh trộn nhiều rủi ro).
- Mỗi perf PR bắt buộc:
  - Có số liệu before/after.
  - Có danh sách route/component bị ảnh hưởng.
  - Có ghi chú rollback.
- Nếu KPI tốt hơn nhưng vỡ luồng chính, xem như thất bại.
- Nếu KPI giảm > 5% ở route critical, chặn merge trừ khi có phê duyệt ngoại lệ.

## 5) Definition of Done cho mỗi bước
Một bước chỉ được xem là hoàn thành khi đạt tất cả:
- Tính đúng chức năng: test và luồng nghiệp vụ quan trọng đều pass.
- Bằng chứng hiệu năng: có tài liệu so sánh metric với baseline.
- Bằng chứng ổn định: không có console error mới, không có visual regression nghiêm trọng.
- Khả năng deploy: production build thành công.

## 6) Thứ tự thực hiện đề xuất (theo tuần)
- Tuần 1: Baseline + Giai đoạn A.1 + A.2
- Tuần 2: Giai đoạn A.3 + B.4
- Tuần 3: Giai đoạn B.5 + C.6
- Tuần 4: Giai đoạn C.7 + tổng kiểm tra hồi quy/hiệu năng

## 7) Mẫu theo dõi (copy cho từng task)
Sử dụng mẫu ngắn này cho mỗi task tối ưu:
- Task:
- Giả thuyết:
- File/component đã thay đổi:
- Mức rủi ro (Thấp/Trung bình/Cao):
- Số liệu trước khi tối ưu:
- Số liệu sau khi tối ưu:
- Kết quả test chức năng:
- Kết quả perf gate:
- Kế hoạch rollback:
- Trạng thái:
