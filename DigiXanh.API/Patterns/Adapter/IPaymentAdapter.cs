using DigiXanh.API.Models;

namespace DigiXanh.API.Patterns.Adapter;

public interface IPaymentAdapter
{
    Task<PaymentResult> ProcessPaymentAsync(Order order, PaymentInfo paymentInfo);
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentUrl { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class PaymentInfo
{
    public string? ReturnUrl { get; set; }
    public string IpAddress { get; set; } = string.Empty;
}
