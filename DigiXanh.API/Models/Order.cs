namespace DigiXanh.API.Models;

public class Order
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
}

public enum OrderStatus
{
    Pending = 0,
    Paid = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}