# 🌱 DigiXanh

DigiXanh là dự án website thương mại điện tử bán cây xanh, được xây dựng theo mô hình tách **Frontend** và **Backend**.

## Cấu trúc repository

- `digixanh-fe/`: Frontend (Angular)
- `DigiXanh.API/`: Backend (ASP.NET Core 8 Web API)

## Công nghệ chính

### Frontend
- Angular 17+
- CoreUI for Angular
- Formly

### Backend
- ASP.NET Core 8 Web API
- Entity Framework Core (Code First)
- SQL Server
- ASP.NET Core Identity + JWT

## Chạy dự án local

### 1) Chạy Backend
```bash
cd DigiXanh.API
dotnet restore
dotnet ef database update
dotnet run
```

- Swagger: `http://localhost:5132/swagger`
- Health check: `GET /api/health`

### 2) Chạy Frontend
```bash
cd digixanh-fe
npm install
npm start
```

- Frontend thường chạy tại: `http://localhost:4200`

## Quy trình làm việc (tóm tắt)

1. Nhận task và đọc kỹ Acceptance Criteria.
2. Đọc tài liệu bắt buộc trong `.github/` (`context.md`, `frontend-instruction.md`, `backend-instruction.md`).
3. Agent thực hiện code + test local + đảm bảo DoD.
4. Agent báo cáo thay đổi và các bước kiểm tra.
5. PO tự quyết định commit/push/branch/PR theo nhu cầu quản lý.

## Ghi chú

- Không commit thư mục build artifacts (`bin/`, `obj/`, `node_modules/`).
- Ưu tiên đồng bộ API contract giữa FE và BE trước khi tích hợp.
- Mặc định agent **không tự thực hiện** commit/push/PR trừ khi PO yêu cầu rõ ràng.
