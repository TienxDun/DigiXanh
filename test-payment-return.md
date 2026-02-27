# Test trang Payment Return

## Test trường hợp THÀNH CÔNG
Mở URL sau trên browser:
```
http://localhost:4200/payment-return?paymentStatus=success&orderId=123&message=Thanh+toan+thanh+cong&transactionId=VNP123456&vnp_ResponseCode=00
```

## Test trường hợp HỦY
```
http://localhost:4200/payment-return?paymentStatus=cancelled&message=Da+huy+thanh+toan&orderId=123&vnp_ResponseCode=24
```

## Test trường hợp THẤT BẠI
```
http://localhost:4200/payment-return?paymentStatus=failed&message=Tai+khoan+khong+du+so+du&vnp_ResponseCode=51
```

## Test qua API thật
Nếu muốn test flow đầy đủ, dùng ngrok:

1. Chạy ngrok: `ngrok http 4200`
2. Copy HTTPS URL
3. Cập nhật `VNPay:FrontendReturnUrl` trong appsettings.json
4. Restart API
5. Thanh toán VNPay
