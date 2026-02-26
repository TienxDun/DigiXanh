using System.Net;
using System.Security.Cryptography;
using System.Text;
using DigiXanh.API.Models;

namespace DigiXanh.API.Patterns.Adapter;

public class VNPayPaymentAdapter : IPaymentAdapter
{
    private readonly IConfiguration _config;

    public VNPayPaymentAdapter(IConfiguration config)
    {
        _config = config;
    }

    public Task<PaymentResult> ProcessPaymentAsync(Order order, PaymentInfo paymentInfo)
    {
        order.PaymentMethod = PaymentMethod.VNPay;
        order.Status = OrderStatus.Pending;

        var vnp_ReturnUrl = paymentInfo.ReturnUrl ?? _config["VNPay:ReturnUrl"] ?? "http://localhost:4200/payment-return";
        var vnp_Url = _config["VNPay:Url"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        var vnp_TmnCode = _config["VNPay:TmnCode"] ?? "";
        var vnp_HashSecret = _config["VNPay:HashSecret"] ?? "";

        var ipAddr = GetIpAddress(paymentInfo.IpAddress);
        var createDate = DateTime.Now;
        var expireDate = createDate.AddMinutes(15);

        // Build danh sách tham số
        var vnpayData = new Dictionary<string, string>
        {
            { "vnp_Version", "2.1.0" },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", vnp_TmnCode },
            { "vnp_Amount", ((long)(order.FinalAmount * 100)).ToString() },
            { "vnp_CreateDate", createDate.ToString("yyyyMMddHHmmss") },
            { "vnp_ExpireDate", expireDate.ToString("yyyyMMddHHmmss") },
            { "vnp_CurrCode", "VND" },
            { "vnp_IpAddr", ipAddr },
            { "vnp_Locale", "vn" },
            { "vnp_OrderInfo", $"Thanh toan don hang {order.Id}" },
            { "vnp_OrderType", "other" },
            { "vnp_ReturnUrl", vnp_ReturnUrl },
            { "vnp_TxnRef", order.Id.ToString() }
        };

        // Sắp xếp theo key
        var sortedData = vnpayData.OrderBy(x => x.Key).ToList();

        // Build raw data để hash - URL encode cả key và value (theo chuẩn VNPay PHP)
        var hashData = new StringBuilder();
        for (int i = 0; i < sortedData.Count; i++)
        {
            var kvp = sortedData[i];
            if (i > 0)
            {
                hashData.Append("&");
            }
            hashData.Append(WebUtility.UrlEncode(kvp.Key));
            hashData.Append("=");
            hashData.Append(WebUtility.UrlEncode(kvp.Value));
        }

        // Tạo chữ ký
        var vnp_SecureHash = HmacSHA512(vnp_HashSecret, hashData.ToString());

        // Build URL query string (không encode vnp_SecureHash)
        var queryBuilder = new StringBuilder();
        for (int i = 0; i < sortedData.Count; i++)
        {
            var kvp = sortedData[i];
            if (i > 0)
            {
                queryBuilder.Append("&");
            }
            queryBuilder.Append(WebUtility.UrlEncode(kvp.Key));
            queryBuilder.Append("=");
            queryBuilder.Append(WebUtility.UrlEncode(kvp.Value));
        }
        queryBuilder.Append("&vnp_SecureHash=");
        queryBuilder.Append(vnp_SecureHash);

        var paymentUrl = $"{vnp_Url}?{queryBuilder}";

        return Task.FromResult(new PaymentResult
        {
            Success = true,
            Message = "Chuyển hướng đến cổng thanh toán VNPay",
            TransactionId = null,
            PaymentUrl = paymentUrl
        });
    }

    private static string GetIpAddress(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip) || ip == "::1")
        {
            return "127.0.0.1";
        }
        
        var ips = ip.Split(',');
        var firstIp = ips[0].Trim();
        
        if (firstIp.Contains(":") && !firstIp.Contains("."))
        {
            return "127.0.0.1";
        }
        
        return firstIp;
    }

    private static string HmacSHA512(string key, string inputData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        
        using (var hmac = new HMACSHA512(keyBytes))
        {
            var hashValue = hmac.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (var b in hashValue)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}

public class VnPayLibrary
{
    public static bool ValidateSignature(Dictionary<string, string> vnpayData, string inputHash, string secretKey)
    {
        // Remove vnp_SecureHash from data
        vnpayData.Remove("vnp_SecureHash");
        vnpayData.Remove("vnp_SecureHashType");
        
        // Sort and build hash data
        var sortedData = vnpayData.OrderBy(x => x.Key).ToList();
        var hashData = new StringBuilder();
        for (int i = 0; i < sortedData.Count; i++)
        {
            var kvp = sortedData[i];
            if (i > 0)
            {
                hashData.Append("&");
            }
            hashData.Append(WebUtility.UrlEncode(kvp.Key));
            hashData.Append("=");
            hashData.Append(WebUtility.UrlEncode(kvp.Value));
        }
        
        var myChecksum = HmacSHA512(secretKey, hashData.ToString());
        return myChecksum.Equals(inputHash, StringComparison.OrdinalIgnoreCase);
    }

    private static string HmacSHA512(string key, string inputData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        
        using (var hmac = new HMACSHA512(keyBytes))
        {
            var hashValue = hmac.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (var b in hashValue)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
