namespace DigiXanh.API.DTOs.Trefle;

public record TreflePlantSearchItemDto(
    int Id,
    string? CommonName,
    string ScientificName,
    string? ImageUrl
);
