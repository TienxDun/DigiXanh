namespace DigiXanh.API.DTOs.Trefle;

public record TreflePlantDetailDto(
    int Id,
    string? Name,
    string ScientificName,
    string? Description,
    string? ImageUrl,
    string? Family,
    string? Genus,
    string? Bibliography,
    string? Author,
    int? Year
);
