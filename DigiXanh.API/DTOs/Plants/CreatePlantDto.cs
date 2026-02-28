using System.ComponentModel.DataAnnotations;

namespace DigiXanh.API.DTOs.Plants;

public record CreatePlantDto(
    [Required]
    [MaxLength(200)]
    string Name,

    [Required]
    [MaxLength(200)]
    string ScientificName,

    string? Description,

    [MaxLength(500)]
    string? ImageUrl,

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    decimal Price,

    [Range(1, int.MaxValue)]
    int CategoryId,

    [Range(0, int.MaxValue)]
    int? StockQuantity = null
);
