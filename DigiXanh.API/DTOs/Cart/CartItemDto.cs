namespace DigiXanh.API.DTOs.Cart;

public record CartItemDto(
    int Id,
    int PlantId,
    string PlantName,
    string ScientificName,
    decimal Price,
    string ImageUrl,
    int Quantity,
    decimal LineTotal,
    int? StockQuantity = null
);

public record CartSummaryDto(
    IReadOnlyList<CartItemDto> Items,
    int TotalQuantity,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal DiscountPercent,
    decimal FinalAmount
);