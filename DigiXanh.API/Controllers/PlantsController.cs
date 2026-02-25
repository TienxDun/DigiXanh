using DigiXanh.API.Constants;
using DigiXanh.API.Data;
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
    [ProducesResponseType(typeof(IReadOnlyCollection<PlantDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var plants = await _dbContext.Plants
            .AsNoTracking()
            .Include(plant => plant.Category)
            .Where(plant => !plant.IsDeleted)
            .OrderByDescending(plant => plant.CreatedAt)
            .Select(plant => new PlantDto(
                plant.Id,
                plant.Name,
                plant.ScientificName,
                plant.Price,
                plant.Category != null ? plant.Category.Name : string.Empty,
                plant.ImageUrl,
                plant.CreatedAt))
            .ToListAsync();

        return Ok(plants);
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
