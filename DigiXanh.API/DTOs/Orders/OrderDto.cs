namespace DigiXanh.API.DTOs.Orders;

/// <summary>
/// DTO cơ bản cho đơn hàng (dùng cho cả Admin và Customer)
/// </summary>
public class OrderDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO chi tiết đơn hàng cho Admin
/// </summary>
public class OrderDetailDto : OrderDto
{
    public string RecipientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
}

/// <summary>
/// DTO chi tiết đơn hàng cho Customer (US19)
/// </summary>
public class CustomerOrderDetailDto : OrderDto
{
    public string RecipientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int PlantId { get; set; }
    public string PlantName { get; set; } = string.Empty;
    public string? ScientificName { get; set; }
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

/// <summary>
/// Request hủy đơn hàng (US20)
/// </summary>
public class CancelOrderRequest
{
    public string? Reason { get; set; }
}

/// <summary>
/// Response sau khi hủy đơn hàng (US20)
/// </summary>
public class CancelOrderResponse
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
