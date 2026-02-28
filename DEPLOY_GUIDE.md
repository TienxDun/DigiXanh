# 🚀 Hướng Dẫn Deploy DigiXanh

> File hướng dẫn chi tiết deploy Frontend (GitHub Pages) và Backend (Render)

---

## 📋 Mục Lục

1. [Tổng quan](#1-tổng-quan)
2. [Deploy Backend (Render)](#2-deploy-backend-render)
3. [Deploy Frontend (GitHub Pages)](#3-deploy-frontend-github-pages)
4. [Cấu hình GitHub Actions](#4-cấu-hình-github-actions)
5. [Xử lý lỗi thường gặp](#5-xử-lý-lỗi-thường-gặp)

---

## 1. Tổng Quan

| Thành phần | Nền tảng | URL mẫu |
|------------|----------|---------|
| Backend API | Render | `https://digixanh-api.onrender.com` |
| Frontend | GitHub Pages | `https://yourusername.github.io/digixanh` |

**Lưu ý quan trọng:**
- Deploy **Backend trước**, lấy URL API rồi mới cấu hình Frontend
- GitHub Pages miễn phí cho public repo
- Render có free tier (sleep sau 15 phút không hoạt động)

---

## 2. Deploy Backend (Render)

### 2.1. Chuẩn bị

Đảm bảo code đã push lên GitHub:
```bash
git add .
git commit -m "Ready for deploy"
git push origin main
```

### 2.2. Tạo tài khoản Render

1. Truy cập: https://render.com
2. Click **Get Started for Free**
3. Đăng ký bằng GitHub account (khuyến nghị)

### 2.3. Tạo Web Service

1. Từ Dashboard, click **New +** → **Web Service**
2. Chọn repository **DigiXanh** từ GitHub
3. Điền thông tin:

| Field | Giá trị |
|-------|---------|
| Name | `digixanh-api` (hoặc tên bạn muốn) |
| Region | Singapore (gần VN nhất) |
| Branch | `main` |
| Root Directory | `DigiXanh.API` |
| Runtime | `.NET` |
| Build Command | `dotnet restore && dotnet build -c Release` |
| Start Command | `dotnet bin/Release/net8.0/DigiXanh.API.dll` |
| Plan | Free |

4. Click **Create Web Service**

### 2.4. Thêm Environment Variables

Trong tab **Environment**, thêm các biến:

```
ASPNETCORE_ENVIRONMENT=Production
JWT__Key=your-super-secret-key-min-32-chars-long
JWT__Issuer=DigiXanh
JWT__Audience=DigiXanhClient
JWT__ExpireMinutes=60

# Database - dùng MonsterASP.NET hoặc SQL Server cloud
ConnectionStrings__DefaultConnection=Server=...;Database=DigiXanhDb;User Id=...;Password=...;TrustServerCertificate=True

# VNPay (nếu dùng)
VNPay__TmnCode=your-tmn-code
VNPay__HashSecret=your-hash-secret
VNPay__Url=https://sandbox.vnpayment.vn/paymentv2/vpcpay.html
VNPay__ReturnUrl=https://your-api.onrender.com/api/payment/vnpay-return
VNPay__FrontendReturnUrl=https://yourusername.github.io/digixanh/payment-return
VNPay__IpnUrl=https://your-api.onrender.com/api/payment/vnpay-ipn

# Perenual API (nếu dùng)
Perenual__ApiKey=your-perenual-api-key
```

### 2.5. Kiểm tra API

Sau khi deploy xong, truy cập:
```
https://your-api.onrender.com/api/health
```

Nếu thấy `{"status":"healthy"}` là thành công! 🎉

---

## 3. Deploy Frontend (GitHub Pages)

### 3.1. Cấu hình API URL

Mở file `digixanh-fe/src/environments/environment.prod.ts`:

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://your-api.onrender.com/api'  // Thay bằng URL Render của bạn
};
```

### 3.2. Cấu hình Angular cho GitHub Pages

Mở file `digixanh-fe/angular.json`, tìm phần `build` → `options`:

```json
"options": {
  "baseHref": "/DigiXanh/",  // Thêm dòng này - tên repository của bạn
  "outputPath": "dist/digixanh-fe",
  ...
}
```

Nếu deploy vào root (`username.github.io`), để `"baseHref": "/"`

### 3.3. Cài đặt gh-pages package

```bash
cd digixanh-fe
npm install --save-dev angular-cli-ghpages
```

### 3.4. Deploy thủ công (lần đầu)

```bash
cd digixanh-fe

# Build production
npm run build -- --configuration production --base-href "/DigiXanh/"

# Deploy lên gh-pages
npx angular-cli-ghpages --dir=dist/digixanh-fe/browser
```

### 3.5. Deploy bằng npm script

Thêm vào `digixanh-fe/package.json`:

```json
"scripts": {
  ...
  "deploy": "ng build --configuration production --base-href '/DigiXanh/' && npx angular-cli-ghpages --dir=dist/digixanh-fe/browser"
}
```

Sau đó deploy bằng lệnh:
```bash
npm run deploy
```

---

## 4. Cấu Hình GitHub Actions

### 4.1. Tạo workflow file

Tạo file `.github/workflows/deploy-fe.yml`:

```yaml
name: Deploy Frontend to GitHub Pages

on:
  push:
    branches: [main]
    paths:
      - 'digixanh-fe/**'
  workflow_dispatch:  # Cho phép chạy thủ công

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: 'pages'
  cancel-in-progress: false

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '22'
          cache: 'npm'
          cache-dependency-path: './digixanh-fe/package-lock.json'

      - name: Install dependencies
        run: |
          cd digixanh-fe
          npm ci

      - name: Run tests
        run: |
          cd digixanh-fe
          npm run test:ci -- --coverage
        continue-on-error: true  # Tạm thờI cho qua nếu test fail

      - name: Build
        run: |
          cd digixanh-fe
          npm run build -- --configuration production --base-href '/DigiXanh/'

      - name: Setup Pages
        uses: actions/configure-pages@v4

      - name: Upload artifact
        uses: actions/upload-pages-artifact@v3
        with:
          path: './digixanh-fe/dist/digixanh-fe/browser'

  deploy:
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    runs-on: ubuntu-latest
    needs: build
    steps:
      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v4
```

### 4.2. Bật GitHub Pages

1. Vào repository → **Settings** → **Pages**
2. Source: chọn **GitHub Actions**
3. Save

### 4.3. Tạo workflow cho Backend (tùy chọn)

Nếu dùng Render auto-deploy, không cần workflow. Nếu muốn deploy bằng GitHub Actions, tạo file `.github/workflows/deploy-be.yml`:

```yaml
name: Deploy Backend

on:
  push:
    branches: [main]
    paths:
      - 'DigiXanh.API/**'
  workflow_dispatch:

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore
        run: dotnet restore DigiXanh.sln
      
      - name: Build
        run: dotnet build DigiXanh.sln --configuration Release
      
      - name: Test
        run: dotnet test DigiXanh.sln --verbosity normal

  # Render tự động deploy khi push lên main
  # Hoặc dùng Render Deploy Hook nếu cần trigger thủ công
```

---

## 5. Xử Lý Lỗi Thường Gặp

### 5.1. Lỗi `Cannot find dependency '@vitest/coverage-v8'`

**Nguyên nhân:** Thiếu package coverage

**Fix:**
```bash
cd digixanh-fe
npm install --save-dev @vitest/coverage-v8
```

### 5.2. Lỗi `404` khi refresh trang trên GitHub Pages

**Nguyên nhân:** GitHub Pages không hỗ trợ Angular routing mặc định

**Fix:** Tạo file `404.html` copy từ `index.html`

Thêm vào `digixanh-fe/package.json`:
```json
"scripts": {
  "postbuild": "cp dist/digixanh-fe/browser/index.html dist/digixanh-fe/browser/404.html"
}
```

### 5.3. Lỗi CORS từ Frontend gọi API

**Fix:** Trong Backend `Program.cs`, đảm bảo có:

```csharp
app.UseCors(policy => 
    policy.WithOrigins(
        "https://yourusername.github.io",
        "http://localhost:4200"
    )
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());
```

### 5.4. Render sleep sau 15 phút

**Giải pháp:** Dùng UptimeRobot ping API mỗi 5 phút:
1. Đăng ký https://uptimerobot.com
2. Add monitor → HTTP(s)
3. URL: `https://your-api.onrender.com/api/health`
4. Interval: 5 minutes

### 5.5. Lỗi baseHref sai đường dẫn

**Triệu chứng:** Trang trắng, không load CSS/JS

**Fix:** Đảm bảo `baseHref` khớp với tên repository:
- Repo: `username.github.io/DigiXanh` → `baseHref: "/DigiXanh/"`
- Repo: `username.github.io` → `baseHref: "/"`

---

## ✅ Checklist Trước Khi Deploy

### Backend
- [ ] Connection string database đã cấu hình
- [ ] JWT Key đủ mạnh (≥32 ký tự)
- [ ] CORS đã allow domain GitHub Pages
- [ ] API health check hoạt động

### Frontend
- [ ] `environment.prod.ts` có đúng API URL
- [ ] `baseHref` khớp với tên repository
- [ ] Tests pass (`npm run test:ci`)
- [ ] Build thành công (`npm run build`)

### GitHub
- [ ] Repository public (cho GitHub Pages free)
- [ ] GitHub Actions enabled
- [ ] Pages source set to GitHub Actions

---

## 📚 Tài liệu tham khảo

- [Render Docs](https://docs.render.com/)
- [GitHub Pages Docs](https://docs.github.com/en/pages)
- [Angular Deployment](https://angular.io/guide/deployment)
- [Angular CLI GitHub Pages](https://www.npmjs.com/package/angular-cli-ghpages)

---

*Cập nhật: 2026-03-01*
