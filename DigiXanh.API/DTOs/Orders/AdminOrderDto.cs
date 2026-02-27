namespace DigiXanh.API.DTOs.Orders;

/// <summary>
/// DTO cho danh sách đơn hàng (Admin view) - Thêm thông tin khách hàng
/// </summary>
public class AdminOrderDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public AdminOrderDto() { }

    public AdminOrderDto(
        int id,
        DateTime orderDate,
        string customerName,
        string customerEmail,
        decimal totalAmount,
        decimal finalAmount,
        string status,
        string paymentMethod,
        string phone)
    {
        Id = id;
        OrderDate = orderDate;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        TotalAmount = totalAmount;
        FinalAmount = finalAmount;
        Status = status;
        PaymentMethod = paymentMethod;
        Phone = phone;
    }
}

/// <summary>
/// DTO chi tiết đơn hàng cho Admin - Bao gồm thông tin giao hàng, items và lịch sử trạng thái
/// </summary>
public class AdminOrderDetailDto : AdminOrderDto
{
    public string ShippingAddress { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();

    public AdminOrderDetailDto() { }

    public AdminOrderDetailDto(
        int id,
        DateTime orderDate,
        string customerName,
        string customerEmail,
        decimal totalAmount,
        decimal finalAmount,
        string status,
        string paymentMethod,
        string phone)
        : base(id, orderDate, customerName, customerEmail, totalAmount, finalAmount, status, paymentMethod, phone)
    {
    }
}
