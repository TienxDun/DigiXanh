namespace DigiXanh.API.DTOs.Plants;

public record PlantDto(
    int Id,
    string Name,
    string ScientificName,
    decimal Price,
    string CategoryName,
    string ImageUrl,
    DateTime CreatedAt,
    int? StockQuantity = null);
