using System.Net;
using System.Security.Cryptography;
using System.Text;
using DigiXanh.API.Models;

namespace DigiXanh.API.Patterns.Adapter;

public class VNPayPaymentAdapter : IPaymentAdapter
{
    private readonly IConfiguration _config;
    private readonly ILogger<VNPayPaymentAdapter> _logger;

    public VNPayPaymentAdapter(IConfiguration config, ILogger<VNPayPaymentAdapter> logger)
    {
        _config = config;
        _logger = logger;
    }

    public Task<PaymentResult> ProcessPaymentAsync(Order order, PaymentInfo paymentInfo)
    {
        order.PaymentMethod = PaymentMethod.VNPay;
        order.Status = OrderStatus.Pending;

        var vnpUrl = (_config["VNPay:Url"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html").Trim();
        var vnpTmnCode = (_config["VNPay:TmnCode"] ?? string.Empty).Trim();
        var vnpHashSecret = (_config["VNPay:HashSecret"] ?? string.Empty).Trim();
        var configuredReturnUrl = (_config["VNPay:ReturnUrl"] ?? string.Empty).Trim();
        var configuredIpnUrl = (_config["VNPay:IpnUrl"] ?? string.Empty).Trim();
        var vnpReturnUrl = ResolveReturnUrl(paymentInfo.ReturnUrl, configuredReturnUrl);

        if (IsMissingOrPlaceholder(vnpTmnCode) || IsMissingOrPlaceholder(vnpHashSecret))
        {
            _logger.LogError("[VNPay-CreateUrl] VNPay credentials are missing or placeholder values. TmnCodeConfigured={HasTmnCode}, HashSecretConfigured={HasHashSecret}",
                !IsMissingOrPlaceholder(vnpTmnCode),
                !IsMissingOrPlaceholder(vnpHashSecret));
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                Message = "Cấu hình VNPay chưa hợp lệ. Vui lòng cập nhật TmnCode/HashSecret thật trong cấu hình môi trường."
            });
        }

        if (!IsValidAbsoluteHttpUrl(vnpUrl))
        {
            _logger.LogError("[VNPay-CreateUrl] Invalid VNPay URL configuration: {VnpUrl}", vnpUrl);
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                Message = "Cấu hình URL cổng VNPay không hợp lệ."
            });
        }

        if (!IsValidAbsoluteHttpUrl(vnpReturnUrl))
        {
            _logger.LogError("[VNPay-CreateUrl] Invalid ReturnUrl configuration. ClientReturnUrl={ClientReturnUrl}, ConfiguredReturnUrl={ConfiguredReturnUrl}",
                paymentInfo.ReturnUrl,
                configuredReturnUrl);
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                Message = "Cấu hình ReturnUrl VNPay không hợp lệ. Vui lòng dùng URL tuyệt đối (http/https)."
            });
        }

        var ipAddr = GetIpAddress(paymentInfo.IpAddress);
        var createDate = GetVietnamTimeNow();
        var expireDate = createDate.AddMinutes(15);

        var vnpay = new VnPayLibrary();
        vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
        vnpay.AddRequestData("vnp_Command", "pay");
        vnpay.AddRequestData("vnp_TmnCode", vnpTmnCode);
        vnpay.AddRequestData("vnp_Amount", ((long)Math.Round(order.FinalAmount * 100m, MidpointRounding.AwayFromZero)).ToString());
        vnpay.AddRequestData("vnp_CreateDate", createDate.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_ExpireDate", expireDate.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", "VND");
        vnpay.AddRequestData("vnp_IpAddr", ipAddr);
        vnpay.AddRequestData("vnp_Locale", "vn");
        vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang:{order.Id}");
        vnpay.AddRequestData("vnp_OrderType", "other");
        vnpay.AddRequestData("vnp_ReturnUrl", vnpReturnUrl);
        vnpay.AddRequestData("vnp_TxnRef", order.Id.ToString());
        if (IsValidAbsoluteHttpUrl(configuredIpnUrl))
        {
            vnpay.AddRequestData("vnp_IpnUrl", configuredIpnUrl);
        }

        _logger.LogInformation(
            "[VNPay-CreateUrl] Generating payment URL. OrderId={OrderId}, Amount={Amount}, ReturnUrl={ReturnUrl}, ClientIp={ClientIp}",
            order.Id,
            order.FinalAmount,
            vnpReturnUrl,
            ipAddr);

        var paymentUrl = vnpay.CreateRequestUrl(vnpUrl, vnpHashSecret);

        _logger.LogInformation(
            "[VNPay-CreateUrl] Payment URL generated. OrderId={OrderId}, UrlHost={UrlHost}",
            order.Id,
            new Uri(paymentUrl).Host);

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

    private static string ResolveReturnUrl(string? clientReturnUrl, string configuredReturnUrl)
    {
        if (IsValidAbsoluteHttpUrl(clientReturnUrl))
        {
            return clientReturnUrl!.Trim();
        }

        return configuredReturnUrl;
    }

    private static bool IsMissingOrPlaceholder(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var normalized = value.Trim();
        return normalized.StartsWith("your-", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("your_", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("example", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("changeme", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValidAbsoluteHttpUrl(string? url)
    {
        return Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri)
               && (uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                   || uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));
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

    private static DateTime GetVietnamTimeNow()
    {
        try
        {
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
        }
        catch
        {
            return DateTime.Now;
        }
    }
}

public class VnPayLibrary
{
    public const string VERSION = "2.1.0";

    private readonly SortedList<string, string> _requestData = new();
    private readonly SortedList<string, string> _responseData = new();

    public void AddRequestData(string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
        {
            _requestData[key] = value;
        }
    }

    public void AddResponseData(string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
        {
            _responseData[key] = value;
        }
    }

    public string? GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var value) ? value : null;
    }

    public string CreateRequestUrl(string baseUrl, string secretKey)
    {
        var query = BuildQueryData(_requestData);
        var secureHash = HmacSHA512(secretKey, query);
        return $"{baseUrl}?{query}&vnp_SecureHash={secureHash}";
    }

    public static bool ValidateSignature(Dictionary<string, string> vnpayData, string inputHash, string secretKey)
    {
        // Sử dụng case-insensitive comparer để đúng chuẩn VNPay
        var sortedData = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in vnpayData)
        {
            if (!item.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (item.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase)
                || item.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Giữ nguyên giá trị (đã URL-encoded từ query string)
            if (!string.IsNullOrWhiteSpace(item.Value))
            {
                sortedData[item.Key] = item.Value;
            }
        }

        // Build query data với giá trị đã URL-encoded (không encode lại)
        var signData = BuildQueryDataRaw(sortedData);
        var myChecksum = HmacSHA512(secretKey, signData);
        return myChecksum.Equals(inputHash, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Build query string từ dữ liệu đã URL-encoded (không encode lại)
    /// </summary>
    private static string BuildQueryDataRaw(SortedList<string, string> data)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < data.Count; i++)
        {
            if (i > 0)
            {
                builder.Append('&');
            }

            // Key luôn cần encode
            builder.Append(WebUtility.UrlEncode(data.Keys[i]));
            builder.Append('=');
            // Value đã được URL-encoded từ query string gốc
            builder.Append(data.Values[i]);
        }

        return builder.ToString();
    }

    private static string BuildQueryData(SortedList<string, string> data)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < data.Count; i++)
        {
            if (i > 0)
            {
                builder.Append('&');
            }

            builder.Append(WebUtility.UrlEncode(data.Keys[i]));
            builder.Append('=');
            builder.Append(WebUtility.UrlEncode(data.Values[i]));
        }

        return builder.ToString();
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
