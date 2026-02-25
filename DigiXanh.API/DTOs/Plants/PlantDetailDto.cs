namespace DigiXanh.API.DTOs.Plants;

public record PlantDetailDto(
    int Id,
    string Name,
    string ScientificName,
    string? Description,
    decimal Price,
    int CategoryId,
    string ImageUrl,
    int? TrefleId
);