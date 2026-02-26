Dưới đây là file **frontend-instruction.md** chi tiết dành cho Frontend Agent, dựa trên các quyết định đã thống nhất trong dự án DigiXanh.

---

# 🌱 DigiXanh – Frontend Instruction (Dành cho FE Agent)

## 1. Vai trò và trách nhiệm
- Xây dựng toàn bộ giao diện người dùng cho DigiXanh bằng **Angular**.
- Phối hợp với Backend Agent để tiêu thụ API, đảm bảo luồng dữ liệu chính xác.
- Áp dụng **CoreUI** làm UI library chính, tuân thủ style guide đã định.
- Đảm bảo chất lượng code, tính nhất quán, responsive và khả năng bảo trì.

## 2. Công nghệ sử dụng
- **Framework:** Angular v17+ (khuyến khích dùng standalone components hoặc module lazyload tuỳ context)
- **UI Library:** CoreUI for Angular (đã bao gồm Bootstrap 5) – [https://coreui.io/angular/](https://coreui.io/angular/)
- **Form Handling:** @ngx-formly/core + @ngx-formly/bootstrap (dynamic forms)
- **Icons:** Font Awesome 6 (free) – gói `@fortawesome/fontawesome-free`
- **HTTP Client:** Angular HttpClient + interceptors (xử lý token, error)
- **State Management:** Service + BehaviorSubject (hoặc signals nếu muốn thử nghiệm)
- **Routing:** Angular Router với lazy-loading cho các module chính
- **Testing:** Jasmine + Karma (unit test), có thể viết cho service và component quan trọng

## 3. Cài đặt và cấu hình ban đầu

### 3.1. Clone template CoreUI
```bash
git clone https://github.com/coreui/coreui-free-angular-admin-template.git digixanh-fe
cd digixanh-fe
npm install
```

### 3.2. Cài đặt các thư viện bổ sung
```bash
npm install @ngx-formly/core @ngx-formly/bootstrap @fortawesome/fontawesome-free
```

### 3.3. Cấu hình Font Awesome trong `angular.json`
```json
"styles": [
  "node_modules/@fortawesome/fontawesome-free/css/all.min.css",
  "src/scss/styles.scss"
],
"scripts": []
```

### 3.4. Tạo thư mục và file SCSS custom
Tạo cấu trúc:
```
src/
└── scss/
    ├── _variables.scss
    ├── _mixins.scss
    ├── _overrides.scss
    ├── _utilities.scss
    └── styles.scss (hoặc dùng file chính của CoreUI và import thêm)
```
Cập nhật `src/scss/styles.scss` (hoặc file chính) để import các file trên sau khi import CoreUI.

**Nội dung các file SCSS tham khảo trong `context.md` (phần Style Guide).**

### 3.5. Cấu hình proxy cho development (gọi API local)
Tạo `proxy.conf.json` ở thư mục gốc:
```json
{
  "/api": {
    "target": "https://localhost:5001",
    "secure": false,
    "changeOrigin": true
  }
}
```
Sau đó chạy `ng serve --proxy-config proxy.conf.json` (hoặc thêm vào `package.json` script).

### 3.6. Cập nhật `environment.ts` và `environment.prod.ts`
```typescript
// environment.ts
export const environment = {
  production: false,
  apiUrl: '/api' // nhờ proxy nên dùng relative
};

// environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://ten-api.onrender.com/api' // URL thật từ Render
};
```

## 4. Cấu trúc thư mục dự án

Dựa trên template CoreUI, nhưng tổ chức lại rõ ràng hơn:

```
src/app/
├── core/                           # Singleton services, guards, interceptors
│   ├── guards/
│   ├── interceptors/
│   ├── services/                   # AuthService, CartService (dùng chung)
│   └── core.module.ts               (nếu dùng module, còn standalone có thể bỏ)
├── shared/                          # Shared components, directives, pipes
│   ├── components/
│   ├── directives/
│   ├── pipes/
│   └── shared.module.ts
├── features/                        # Các module tính năng (lazy-load)
│   ├── auth/                        # Đăng nhập, đăng ký
│   │   ├── pages/
│   │   ├── components/
│   │   └── auth-routing.module.ts
│   ├── public/                       # Trang chủ, danh sách cây, chi tiết cây
│   │   ├── pages/
│   │   ├── components/
│   │   └── public-routing.module.ts
│   ├── cart/                          # Giỏ hàng, thanh toán
│   │   ├── pages/
│   │   ├── components/
│   │   └── cart-routing.module.ts
│   └── admin/                         # Quản lý cây, dashboard (bảo vệ bởi AuthGuard)
│       ├── pages/
│       ├── components/
│       └── admin-routing.module.ts
├── app-routing.module.ts
├── app.component.*
└── app.module.ts (hoặc app.config.ts nếu dùng standalone)
```

**Lưu ý:** CoreUI đã có sẵn layout (DefaultLayout) với sidebar và header. Cần tích hợp các module con vào layout này. Xem hướng dẫn của CoreUI để cấu hình routing phù hợp.

## 5. Coding Standards & Convention

### 5.1. Đặt tên
- **Class:** PascalCase (ví dụ: `PlantListComponent`)
- **File:** kebab-case (ví dụ: `plant-list.component.ts`)
- **Service:** PascalCase + 'Service' (ví dụ: `PlantService`)
- **Interface/Model:** PascalCase (ví dụ: `Plant`, `User`)
- **Biến, phương thức:** camelCase
- **Observables:** kết thúc bằng `$` (ví dụ: `plants$`)

### 5.2. Tổ chức code trong component
- Sử dụng `OnPush` change detection strategy nếu có thể.
- Khai báo `@Input()` và `@Output()` rõ ràng.
- Tách logic vào service, component chỉ giữ vai trò presentation.
- Sử dụng async pipe trong template thay vì subscribe thủ công.

### 5.3. Báo cáo thay đổi
- Sau khi hoàn tất task, liệt kê rõ danh sách file đã sửa.
- Ghi kèm cách test local để PO kiểm tra nhanh.
- Nếu PO yêu cầu commit thủ công, có thể đề xuất message: `[USxx] Mô tả ngắn gọn`.

### 5.4. Quy ước Git
- Mặc định FE Agent **không tự tạo nhánh/commit/push/PR**.
- PO là người quản lý Git workflow; agent chỉ thao tác Git khi PO yêu cầu rõ ràng.

## 6. Routing & Guards

### 6.1. Cấu hình routing chính (`app-routing.module.ts`)
```typescript
const routes: Routes = [
  {
    path: '',
    component: DefaultLayoutComponent, // layout của CoreUI
    children: [
      { path: '', loadChildren: () => import('./features/public/public.module').then(m => m.PublicModule) },
      { path: 'auth', loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule) },
      { path: 'cart', loadChildren: () => import('./features/cart/cart.module').then(m => m.CartModule) },
      { path: 'admin', loadChildren: () => import('./features/admin/admin.module').then(m => m.AdminModule), canLoad: [AuthGuard] }
    ]
  },
  { path: '**', redirectTo: '' }
];
```

### 6.2. AuthGuard
- Kiểm tra xem user đã đăng nhập chưa và có role Admin không.
- Sử dụng `canLoad` để chặn lazy module admin nếu không phải admin.
- Có thể dùng `canActivate` cho các route cụ thể.

## 7. State Management

Vì dự án quy mô vừa, sử dụng **Service + BehaviorSubject** là đủ.

### 7.1. AuthService
- Quản lý trạng thái đăng nhập, thông tin user, token.
- Sử dụng `BehaviorSubject<boolean>` cho trạng thái auth.
- Các phương thức: `login()`, `register()`, `logout()`, `getToken()`, `refreshToken()`.

### 7.2. CartService
- Quản lý giỏ hàng (lưu trên server, nhưng có thể cache local).
- `BehaviorSubject<Cart>` cho giỏ hàng hiện tại.
- Các phương thức: `addToCart()`, `updateQuantity()`, `removeFromCart()`, `clearCart()`, `checkout()`.

### 7.3. Sử dụng signals (Angular 17+)
Có thể thay thế BehaviorSubject bằng signals nếu muốn thử nghiệm, nhưng vẫn cần service để chia sẻ dữ liệu.

## 8. Forms với ngx-formly

### 8.1. Cấu hình Formly trong module
```typescript
import { FormlyModule } from '@ngx-formly/core';
import { FormlyBootstrapModule } from '@ngx-formly/bootstrap';

@NgModule({
  imports: [
    FormlyModule.forRoot({
      validationMessages: [
        { name: 'required', message: 'Trường này bắt buộc' },
        { name: 'email', message: 'Email không hợp lệ' }
      ]
    }),
    FormlyBootstrapModule
  ]
})
```

### 8.2. Ví dụ form đăng nhập
```typescript
form = new FormGroup({});
fields: FormlyFieldConfig[] = [
  {
    key: 'email',
    type: 'input',
    props: {
      label: 'Email',
      placeholder: 'Nhập email',
      required: true,
      type: 'email'
    }
  },
  {
    key: 'password',
    type: 'input',
    props: {
      label: 'Mật khẩu',
      placeholder: 'Nhập mật khẩu',
      required: true,
      type: 'password'
    }
  }
];

onSubmit(model: any) {
  this.authService.login(model).subscribe(...);
}
```
Template:
```html
<form [formGroup]="form" (ngSubmit)="onSubmit(model)">
  <formly-form [form]="form" [fields]="fields" [(model)]="model"></formly-form>
  <button type="submit" class="btn btn-primary">Đăng nhập</button>
</form>
```

## 9. Tích hợp API và Interceptors

### 9.1. HttpInterceptor cho token
```typescript
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler) {
    const token = this.authService.getToken();
    if (token) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    }
    return next.handle(req);
  }
}
```

### 9.2. ErrorInterceptor
- Bắt lỗi HTTP (401, 403, 500, ...) và hiển thị thông báo (dùng CoreUI toast hoặc alert).
- Xử lý logout nếu 401.

### 9.3. Service gọi API
```typescript
@Injectable({ providedIn: 'root' })
export class PlantService {
  constructor(private http: HttpClient) {}

  getPlants(params?: any): Observable<Plant[]> {
    return this.http.get<Plant[]>(`${environment.apiUrl}/plants`, { params });
  }

  getPlant(id: number): Observable<Plant> {
    return this.http.get<Plant>(`${environment.apiUrl}/plants/${id}`);
  }

  // Admin
  createPlant(data: FormData): Observable<Plant> {
    return this.http.post<Plant>(`${environment.apiUrl}/admin/plants`, data);
  }

  updatePlant(id: number, data: FormData): Observable<Plant> {
    return this.http.put<Plant>(`${environment.apiUrl}/admin/plants/${id}`, data);
  }

  deletePlant(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/admin/plants/${id}`);
  }

  // Trefle search (Admin)
  searchTrefle(query: string): Observable<any> {
    return this.http.get(`${environment.apiUrl}/admin/trefle/search`, { params: { q: query } });
  }
}
```

## 10. Styling & Theming

### 10.1. Sử dụng CoreUI classes
- Tận dụng các class có sẵn của Bootstrap và CoreUI (`.btn`, `.card`, `.table`, v.v.)
- Khi cần custom, viết SCSS theo BEM và đặt trong file riêng của component.

### 10.2. Dark mode
- CoreUI hỗ trợ dark mode qua attribute `data-bs-theme="dark"`. Có thể thêm toggle switch để thay đổi.
- Trong component, thay đổi attribute trên `body` hoặc `html`.

### 10.3. Responsive
- Sử dụng grid Bootstrap: `col-*`, `row`, v.v.
- Test trên các kích thước màn hình (mobile, tablet, desktop).

### 10.4. Nguyên tắc "Compact Premium UI"
Dự án hướng tới giao diện gọn gàng, tinh tế và cao cấp, vừa vặn trong một khung hình (Compact View) mà không cần cuộn (scroll) quá nhiều:
- **Từ bỏ White-space thừa thãi**: Sử dụng margin/padding nhỏ & vừa (`p-2`, `p-3`, `mb-3`, `gap-2`) thay vì các lớp `py-5`, `mb-5`, `g-5` tạo khoảng trắng quá rộng.
- **Typography (Cỡ chữ)**: 
  - Khuyến khích dùng heading nhỏ nhắn tinh tế (`fs-3`, `fs-4`, `fs-5`) thay vì `display-4` hoặc `display-6`.
  - Các chú thích, thông tin phụ hoặc mô tả khoa học luôn dùng size nhỏ (`small`, `fs-6`, `0.8rem` - `0.9rem`).
- **Kích thước phần tử (Elements Size)**:
  - Cố định ảnh nhỏ: Giỏ hàng chỉ dùng `56px` (hoặc `48px` web mobile), ảnh thẻ sản phẩm cao ~`160px`, ảnh chi tiết cây to nhất không quá `400px`.
  - Các input, button: Dùng `py-1`, `py-2`, `px-3` hoặc `btn-sm`. Thu nhỏ các nút bấm chức năng (ví dụ nút (+)/(-) chỉ `28px` - `32px` với icon siêu bé).
- **Thẩm mỹ (Aesthetics)**: Sử dụng góc bo mềm mại nhưng tinh tế (`rounded-3`, `rounded-4`), kèm đổ bóng mờ nhẹ nhàng (`shadow-sm`) để hiện rõ nét "Premium" thay vì flat design thô cứng. Bo tròn mọi Card/Nút bấm để tạo ấn tượng thân thiện thiên nhiên.

## 11. Testing

### 11.1. Unit test với Jasmine/Karma
- Viết test cho các service quan trọng (AuthService, CartService) và các pipe.
- Test component cơ bản (kiểm tra render, event).
- Dùng `HttpClientTestingController` để mock API.

### 11.2. Kiểm thử manual
- Chạy trên nhiều trình duyệt (Chrome, Firefox).
- Đảm bảo responsive.
- Kiểm tra các luồng lỗi (nhập sai, không có quyền).

## 12. Triển khai lên GitHub Pages

### 12.1. Build và deploy
- Build: `ng build --prod --base-href /ten-repo/` (nếu deploy dưới dạng project site)
- Có thể dùng GitHub Actions để tự động deploy khi push lên nhánh `main`.

### 12.2. Cấu hình GitHub Pages
- Vào Settings > Pages, chọn branch `gh-pages` (hoặc thư mục docs).
- Đảm bảo `index.html` được phục vụ đúng.

### 12.3. CORS
- Backend phải cho phép origin của GitHub Pages (ví dụ: `https://username.github.io`).
- Nếu dùng proxy trong dev, ở production cần đảm bảo API URL chính xác.

## 13. Phối hợp với Backend Agent

### 13.1. Thống nhất API contract
- Trước khi code, hai Agent cần trao đổi để xác định:
  - Endpoint (URL, method)
  - Request body / query params
  - Response structure (DTO)
  - Authentication required (token trong header)
- Có thể dùng file `.http` hoặc Postman collection để lưu lại.

### 13.2. Mock API nếu BE chưa xong
- Tạo service với mock data (dùng `of()` và delay) để FE vẫn chạy được.

### 13.3. Xử lý lỗi
- Khi API trả về lỗi, hiển thị thông báo thân thiện với người dùng (dùng toast của CoreUI).

## 14. Quy trình làm việc cụ thể khi nhận task

1. **Đọc task** từ PO (ví dụ: US01 - Đăng ký tài khoản).
2. **Đọc lại context.md** và **frontend-instruction.md** để nắm vững quy tắc.
3. **Trao đổi với BE agent** nếu task liên quan đến API để thống nhất contract.
4. **Phát triển**:
   - Tạo component mới trong module phù hợp (auth).
   - Viết form với ngx-formly.
   - Viết service gọi API.
   - Xử lý response/error.
   - Đảm bảo style theo đúng thiết kế.
5. **Tự kiểm thử** trên local.
6. **Bàn giao cho PO**:
  - Tóm tắt thay đổi.
  - Liệt kê file đã sửa.
  - Gửi hướng dẫn test và đề xuất commit message (nếu cần).
7. **Phản hồi feedback**, sửa nếu cần.
8. **Cập nhật board** khi PO yêu cầu.

---

**Chúc bạn làm việc hiệu quả và có những trải nghiệm học hỏi thú vị! Nếu có thắc mắc, hãy hỏi PO ngay nhé.** 🌿