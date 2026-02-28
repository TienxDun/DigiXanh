using DigiXanh.API.Constants;
using DigiXanh.API.Data;
using DigiXanh.API.DTOs.Common;
using DigiXanh.API.DTOs.Plants;
using DigiXanh.API.Helpers;
using DigiXanh.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiXanh.API.Controllers;

[ApiController]
[Route("api/admin/plants")]
[Authorize(Roles = DefaultRoles.Admin)]
public class AdminPlantsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public AdminPlantsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PagedResult<PlantDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlants(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        var query = _dbContext.Plants
            .AsNoTracking()
            .Include(plant => plant.Category)
            .Where(plant => !plant.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLower();
            query = query.Where(plant =>
                plant.Name.ToLower().Contains(keyword) ||
                plant.ScientificName.ToLower().Contains(keyword));
        }

        var totalCount = await query.CountAsync();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(plant => plant.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(plant => new PlantDto(
                plant.Id,
                plant.Name,
                plant.ScientificName,
                plant.Price,
                plant.Category != null ? plant.Category.Name : string.Empty,
                ImageUrlSanitizer.NormalizeOrEmpty(plant.ImageUrl),
                plant.CreatedAt,
                plant.StockQuantity))
            .ToListAsync();

        return Ok(new PagedResult<PlantDto>(items, totalCount, page, pageSize, totalPages));
    }

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PlantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePlant([FromBody] CreatePlantDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var categoryExists = await _dbContext.Categories
            .AsNoTracking()
            .AnyAsync(category => category.Id == dto.CategoryId);

        if (!categoryExists)
        {
            return BadRequest(new { message = "CategoryId does not exist." });
        }

        var plant = new Plant
        {
            Name         = dto.Name.Trim(),
            ScientificName = dto.ScientificName?.Trim() ?? string.Empty,
            Description  = dto.Description?.Trim(),
            Price        = dto.Price,
            CategoryId   = dto.CategoryId,
            ImageUrl     = ImageUrlSanitizer.NormalizeOrEmpty(dto.ImageUrl),
            TrefleId     = dto.TrefleId,
            StockQuantity = dto.StockQuantity,
            IsDeleted    = false,
            CreatedAt    = DateTime.UtcNow
        };

        _dbContext.Plants.Add(plant);
        await _dbContext.SaveChangesAsync();

        // Reload with Category
        await _dbContext.Entry(plant).Reference(p => p.Category).LoadAsync();

        var result = new PlantDto(
            plant.Id,
            plant.Name,
            plant.ScientificName,
            plant.Price,
            plant.Category?.Name ?? string.Empty,
            ImageUrlSanitizer.NormalizeOrEmpty(plant.ImageUrl),
            plant.CreatedAt,
            plant.StockQuantity
        );

        return CreatedAtAction(nameof(GetPlants), new { id = plant.Id }, result);
    }

    [HttpGet("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PlantDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlantById([FromRoute] int id)
    {
        var plant = await _dbContext.Plants
            .AsNoTracking()
            .Where(item => item.Id == id && !item.IsDeleted)
            .Select(item => new PlantDetailDto(
                item.Id,
                item.Name,
                item.ScientificName,
                item.Description,
                item.Price,
                item.CategoryId ?? 0,
                item.Category != null ? item.Category.Name : string.Empty,
                ImageUrlSanitizer.NormalizeOrEmpty(item.ImageUrl),
                item.TrefleId,
                item.StockQuantity
            ))
            .FirstOrDefaultAsync();

        if (plant is null)
        {
            return NotFound();
        }

        return Ok(plant);
    }

    [HttpPut("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PlantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePlant([FromRoute] int id, [FromBody] CreatePlantDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var categoryExists = await _dbContext.Categories
            .AsNoTracking()
            .AnyAsync(category => category.Id == dto.CategoryId);

        if (!categoryExists)
        {
            return BadRequest(new { message = "CategoryId does not exist." });
        }

        var plant = await _dbContext.Plants
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);

        if (plant is null)
        {
            return NotFound();
        }

        plant.Name = dto.Name.Trim();
        plant.ScientificName = dto.ScientificName.Trim();
        plant.Description = dto.Description?.Trim();
        plant.Price = dto.Price;
        plant.CategoryId = dto.CategoryId;
        plant.ImageUrl = ImageUrlSanitizer.NormalizeOrEmpty(dto.ImageUrl);
        plant.TrefleId = dto.TrefleId;
        plant.StockQuantity = dto.StockQuantity;

        await _dbContext.SaveChangesAsync();

        await _dbContext.Entry(plant).Reference(item => item.Category).LoadAsync();

        var result = new PlantDto(
            plant.Id,
            plant.Name,
            plant.ScientificName,
            plant.Price,
            plant.Category?.Name ?? string.Empty,
            ImageUrlSanitizer.NormalizeOrEmpty(plant.ImageUrl),
            plant.CreatedAt,
            plant.StockQuantity
        );

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoftDeletePlant([FromRoute] int id)
    {
        var plant = await _dbContext.Plants
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);

        if (plant is null)
        {
            return NotFound();
        }

        plant.IsDeleted = true;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("bulk-soft-delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkSoftDeletePlants([FromBody] BulkSoftDeletePlantsDto dto)
    {
        if (dto.Ids is null || dto.Ids.Count == 0)
        {
            return BadRequest(new { message = "Ids is required." });
        }

        var ids = dto.Ids
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        if (ids.Length == 0)
        {
            return BadRequest(new { message = "No valid ids provided." });
        }

        var plants = await _dbContext.Plants
            .Where(item => ids.Contains(item.Id) && !item.IsDeleted)
            .ToListAsync();

        if (plants.Count == 0)
        {
            return NoContent();
        }

        foreach (var plant in plants)
        {
            plant.IsDeleted = true;
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
