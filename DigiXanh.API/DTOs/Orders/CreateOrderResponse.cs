namespace DigiXanh.API.DTOs.Orders;

public class CreateOrderResponse
{
    public bool Success { get; set; }
    public int OrderId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? PaymentUrl { get; set; }
    public OrderDetailDto? Order { get; set; }
}

public class VNPayReturnResponse
{
    public bool Success { get; set; }
    public int OrderId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public OrderDetailDto? Order { get; set; }
}

public class VNPayIpnResponse
{
    public string RspCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
