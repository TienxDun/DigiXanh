namespace DigiXanh.API.DTOs.Perenual;

public record PerenualPlantSearchItemDto(
    int Id,
    string? CommonName,
    string ScientificName,
    string? ImageUrl
);
