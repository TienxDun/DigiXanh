namespace DigiXanh.API.Models;

public class Plant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int? TrefleId { get; set; }
    public int? StockQuantity { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
