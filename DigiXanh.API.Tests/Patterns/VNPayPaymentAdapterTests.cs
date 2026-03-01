using DigiXanh.API.Models;
using DigiXanh.API.Patterns.Adapter;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace DigiXanh.API.Tests.Patterns;

public class VNPayPaymentAdapterTests
{
    [Fact]
    public async Task ProcessPaymentAsync_ReturnsFailure_WhenCredentialsArePlaceholderValues()
    {
        var adapter = CreateAdapter(new Dictionary<string, string?>
        {
            ["VNPay:Url"] = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
            ["VNPay:TmnCode"] = "your-tmn-code",
            ["VNPay:HashSecret"] = "your-hash-secret",
            ["VNPay:ReturnUrl"] = "https://localhost:5001/api/payment/vnpay-return"
        });

        var result = await adapter.ProcessPaymentAsync(CreateOrder(), new PaymentInfo { IpAddress = "::1" });

        Assert.False(result.Success);
        Assert.Contains("Cấu hình VNPay chưa hợp lệ", result.Message);
        Assert.Null(result.PaymentUrl);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ReturnsFailure_WhenReturnUrlIsInvalid()
    {
        var adapter = CreateAdapter(new Dictionary<string, string?>
        {
            ["VNPay:Url"] = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
            ["VNPay:TmnCode"] = "TESTCODE",
            ["VNPay:HashSecret"] = "test-secret",
            ["VNPay:ReturnUrl"] = "/payment-return"
        });

        var result = await adapter.ProcessPaymentAsync(CreateOrder(), new PaymentInfo { IpAddress = "127.0.0.1" });

        Assert.False(result.Success);
        Assert.Contains("ReturnUrl", result.Message);
        Assert.Null(result.PaymentUrl);
    }

    [Fact]
    public async Task ProcessPaymentAsync_GeneratesPaymentUrl_WithConfiguredReturnUrlAndIpnUrl()
    {
        var adapter = CreateAdapter(new Dictionary<string, string?>
        {
            ["VNPay:Url"] = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
            ["VNPay:TmnCode"] = "TESTCODE",
            ["VNPay:HashSecret"] = "test-secret",
            ["VNPay:ReturnUrl"] = "https://localhost:5001/api/payment/vnpay-return",
            ["VNPay:IpnUrl"] = "https://localhost:5001/api/payment/vnpay-ipn"
        });

        var result = await adapter.ProcessPaymentAsync(CreateOrder(), new PaymentInfo
        {
            IpAddress = "::1",
            ReturnUrl = "/invalid-client-url"
        });

        Assert.True(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.PaymentUrl));

        var uri = new Uri(result.PaymentUrl!);
        var query = QueryHelpers.ParseQuery(uri.Query);

        Assert.Equal("https://localhost:5001/api/payment/vnpay-return", query["vnp_ReturnUrl"].ToString());
        Assert.False(query.ContainsKey("vnp_IpnUrl"));
        Assert.Equal("127.0.0.1", query["vnp_IpAddr"].ToString());
    }

    private static VNPayPaymentAdapter CreateAdapter(IDictionary<string, string?> values)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return new VNPayPaymentAdapter(configuration, NullLogger<VNPayPaymentAdapter>.Instance);
    }

    private static Order CreateOrder()
    {
        return new Order
        {
            Id = 1001,
            FinalAmount = 100_000m,
            PaymentMethod = PaymentMethod.VNPay,
            Status = OrderStatus.Pending,
            RecipientName = "Tester",
            Phone = "0900000000",
            ShippingAddress = "Ho Chi Minh City"
        };
    }
}
