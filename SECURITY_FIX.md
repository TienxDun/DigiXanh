# 🔒 Hướng Dẫn Xử Lý Key Bị Lộ

> **CẢNH BÁO**: Các API key đã bị lộ trong Git history và cần được thay thế NGAY LẬP TỨC!

---

## ⚠️ Các Key Đã Bị Lộ

| Dịch vụ | Key | Mức độ nguy hiểm |
|---------|-----|------------------|
| Perenual API | `[REDACTED]` | Cao - Có thể bị lạm dụng API quota |
| VNPay Sandbox | `[REDACTED]` | Cao - Có thể bị giả mạo giao dịch |
| JWT Secret | `DigiXanh-Super-Secret-Key-For-Dev-Only-2026` | Trung bình - Có thể tạo fake token |

---

## 🚨 Các Bước Xử Lý Khẩn Cấp

### Bước 1: Vô hiệu hóa key cũ (LÀM NGAY)

#### 1.1 Perenual API Key
1. Vào https://perenual.com/docs/api
2. Đăng nhập vào dashboard
3. Xóa hoặc vô hiệu hóa key cũ (key đã bị lộ trong commit history)
4. Tạo key mới

#### 1.2 VNPay Sandbox
1. Liên hệ VNPay support hoặc vào merchant portal
2. Yêu cầu reset HashSecret và TmnCode
3. Cập nhật cấu hình mới

### Bước 2: Cập nhật local development

Tạo file `DigiXanh.API/appsettings.Development.json` (đã được gitignore):

```json
{
  "Jwt": {
    "Key": "your-new-dev-key-min-32-characters-here"
  },
  "AdminSeed": {
    "Password": "Your-New-Admin-Password"
  },
  "Perenual": {
    "ApiKey": "your-new-perenual-key"
  },
  "VNPay": {
    "TmnCode": "your-new-tmn-code",
    "HashSecret": "your-new-hash-secret"
  }
}
```

### Bước 3: Cập nhật production (Render)

Trong Dashboard Render, update Environment Variables:

```
JWT__Key=your-new-production-key-min-32-chars
Perenual__ApiKey=your-new-perenual-key
VNPay__TmnCode=your-new-tmn-code
VNPay__HashSecret=your-new-hash-secret
```

### Bước 4: Xoá key khỏi Git History (Tùy chọn nhưng khuyến nghị)

**Lưu ý**: Việc này sẽ rewrite history, ảnh hưởng đến team nếu có nhiều ngườI làm việc.

```bash
# Cài git-filter-repo (nếu chưa có)
pip install git-filter-repo

# Xoá file appsettings.json khỏi history
git filter-repo --path DigiXanh.API/appsettings.json --invert-paths

# Hoặc xoá cụ thể dòng chứa key
git filter-repo --replace-text <(echo "YOUR_OLD_KEY_HERE==>YOUR_NEW_KEY_HERE")
```

Nếu không muốn rewrite history, hãy đảm bảo đã thay thế key cũ bằng key mới ở bước 1.

---

## ✅ Quy trình quản lý Secrets đúng cách

### Local Development
```
DigiXanh.API/
├── appsettings.json           # ✅ Giữ trong repo, dùng placeholder
├── appsettings.Development.json  # ❌ Gitignore, chỉ ở local
├── appsettings.Production.json   # ❌ Gitignore, chỉ ở server
└── appsettings.Example.json      # ✅ Mẫu cho dev mới
```

### Production (Render)
- Không bao giờ commit file `appsettings.Production.json`
- Luôn dùng Environment Variables trong Render Dashboard

### Team Collaboration
1. Dev mới clone repo → copy `appsettings.Example.json` → `appsettings.Development.json`
2. Hỏi lead/devops để lấy key development
3. Không bao giờ commit file chứa key thật

---

## 🔍 Kiểm tra lại sau khi fix

```bash
# Kiểm tra xem còn key nào trong code không
grep -r "sk-" --include="*.json" --include="*.cs"
grep -r "HashSecret" --include="*.json" --include="*.cs" | grep -v "Example"

# Kiểm tra git history
git log --all --full-history --source -S 'YOUR_OLD_KEY_PATTERN'
```

---

## 📋 Checklist

- [ ] Perenual key cũ đã bị vô hiệu hóa
- [ ] VNPay credentials mới đã được cấp
- [ ] JWT key mới đã tạo (≥32 ký tự)
- [ ] Render env vars đã update
- [ ] Local `appsettings.Development.json` đã tạo
- [ ] Test API với key mới hoạt động
- [ ] (Tùy chọn) Git history đã được clean

---

## 🆘 Liên hệ hỗ trợ

- Perenual API: https://perenual.com/support
- VNPay Sandbox: https://sandbox.vnpayment.vn

---

*Cập nhật: 2026-03-01*
