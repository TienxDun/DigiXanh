# 🌱 DigiXanh - Website Thương Mại Điện Tử Bán Cây Xanh

> **Full-stack e-commerce application** với kiến trúc tách biệt Frontend-Backend, áp dụng các Design Patterns và quy trình testing chuyên nghiệp.

---

## 📌 Tổng quan dự án

| | |
|:---|:---|
| **Mô tả** | Nền tảng mua bán cây xanh trực tuyến |
| **Mô hình** | Micro-frontend & API-based architecture |
| **Vai trò của tôi** | Fullstack + DevOps |

---

## 🏗️ Kiến trúc hệ thống

```
┌─────────────────────────────────────────────────────────┐
│  Frontend (Angular 21)         Backend (ASP.NET Core 8) │
│  ├─ Customer Portal            ├─ REST API             │
│  ├─ Admin Dashboard            ├─ JWT Authentication    │
│  └─ Mobile Responsive          ├─ SQL Server Database   │
│                                └─ VNPay Integration     │
└─────────────────────────────────────────────────────────┘
```

---

## 🛠️ Tech Stack

### Frontend
| Công nghệ | Phiên bản | Mục đích |
|-----------|-----------|----------|
| **Angular** | 21.1.5+ | SPA, Component architecture |
| **TypeScript** | 5.9 | Type-safe development |
| **RxJS** | 7.8 | Reactive programming |
| **CoreUI** | 5.6 | Enterprise UI components |
| **Vitest** | 4.0 | Unit testing |
| **Cypress** | 13 | E2E testing |

### Backend
| Công nghệ | Phiên bản | Mục đích |
|-----------|-----------|----------|
| **ASP.NET Core** | 8.0 | Web API framework |
| **Entity Framework** | 8.0 | ORM, Code-First |
| **SQL Server** | 2022 | Database |
| **xUnit** | 2.4 | Unit testing |

### DevOps
| Công nghệ | Mục đích |
|-----------|----------|
| **GitHub Actions** | CI/CD Pipeline |
| **Render** | Backend hosting |
| **GitHub Pages** | Frontend hosting |

---

## ✨ Key Features

### 🛒 Customer Portal
- Product catalog với tìm kiếm & filter
- Shopping cart với real-time updates
- Checkout 2 phương thức: **Cash** & **VNPay**
- Order tracking & history

### ⚙️ Admin Dashboard
- CRUD Products & Categories
- Order management & status updates
- Analytics dashboard (Chart.js)
- User & role management

### 🔐 Bảo mật
- JWT Authentication + Refresh Token
- Role-based Authorization
- XSS/SQL Injection protection

---

## 🎨 Design Patterns Áp Dụng

| Pattern | Application | Benefit |
|---------|-------------|---------|
| **Adapter** | Payment module | Dễ thêm phương thức thanh toán mớI |
| **Decorator** | Discount calculation | Stackable discount rules |
| **Facade** | Order processing | Simplify complex workflow |
| **Repository + Unit of Work** | Data layer | Transaction integrity |

---

## 📊 Kết quả đạt được

### Testing Metrics
```
✅ Unit Tests:         93/95 passed (97.9%)
✅ API Coverage:       100%
✅ E2E Scenarios:      16 critical paths
✅ Code Coverage:      Backend 85% | Frontend 70%
```

### Performance
```
✅ Page Load:          1.2s (Target: <2s)
✅ API Response (p95): 320ms (Target: <500ms)
✅ Concurrent Users:   250+ (Target: 200)
✅ Security Audit:     0 vulnerabilities
```

---

## 🔗 Demo & Source

| Resource | Link |
|----------|------|
| 🌐 Live Demo | 'tienxdun.github.io/DigiXanh/' |
| 📚 API Docs | `https://localhost:5001/swagger` |
| 💻 Source Code | `https://github.com/TienxDun/DigiXanh` |

---
