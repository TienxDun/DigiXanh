using DigiXanh.API.Constants;
using DigiXanh.API.Data;
using DigiXanh.API.DTOs.Common;
using DigiXanh.API.DTOs.Plants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiXanh.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlantsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public PlantsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PagedResult<PlantDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? sortBy = null)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 12 : pageSize;

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

        if (categoryId is > 0)
        {
            query = query.Where(plant => plant.CategoryId == categoryId);
        }

        query = (sortBy ?? string.Empty).Trim().ToLower() switch
        {
            "priceasc" => query.OrderBy(plant => plant.Price).ThenBy(plant => plant.Name),
            "pricedesc" => query.OrderByDescending(plant => plant.Price).ThenBy(plant => plant.Name),
            "nameasc" => query.OrderBy(plant => plant.Name),
            _ => query.OrderByDescending(plant => plant.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var plants = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(plant => new PlantDto(
                plant.Id,
                plant.Name,
                plant.ScientificName,
                plant.Price,
                plant.Category != null ? plant.Category.Name : string.Empty,
                plant.ImageUrl,
                plant.CreatedAt))
            .ToListAsync();

        return Ok(new PagedResult<PlantDto>(plants, totalCount, page, pageSize, totalPages));
    }

    [HttpPost]
    [Authorize(Roles = DefaultRoles.Admin)]
    public IActionResult CreatePlant([FromBody] object payload)
    {
        return Ok(new
        {
            message = "Plant created by admin",
            payload
        });
    }
}
