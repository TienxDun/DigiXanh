namespace DigiXanh.API.DTOs.Cart;

public class AddCartItemRequest
{
    public int PlantId { get; set; }
    public int Quantity { get; set; } = 1;
}