namespace DigiXanh.API.DTOs.Perenual;

public record PerenualPlantDetailDto(
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
