namespace DigiXanh.API.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int PlantId { get; set; }
    public Plant? Plant { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
