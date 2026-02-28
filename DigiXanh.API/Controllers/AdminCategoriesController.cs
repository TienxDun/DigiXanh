using DigiXanh.API.Constants;
using DigiXanh.API.Data;
using DigiXanh.API.DTOs.Categories;
using DigiXanh.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiXanh.API.Controllers;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = DefaultRoles.Admin)]
public class AdminCategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public AdminCategoriesController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryDto(category.Id, category.Name))
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById([FromRoute] int id)
    {
        var category = await _dbContext.Categories
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new CategoryDto(item.Id, item.Name))
            .FirstOrDefaultAsync();

        if (category is null)
        {
            return NotFound(new { message = "Không tìm thấy danh mục." });
        }

        return Ok(category);
    }

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] UpsertCategoryRequest request)
    {
        var normalizedName = NormalizeName(request.Name);
        if (normalizedName is null)
        {
            return BadRequest(new { message = "Tên danh mục là bắt buộc." });
        }

        var duplicated = await _dbContext.Categories
            .AsNoTracking()
            .AnyAsync(item => item.Name.ToLower() == normalizedName.ToLower());

        if (duplicated)
        {
            return BadRequest(new { message = "Tên danh mục đã tồn tại." });
        }

        var category = new Category
        {
            Name = normalizedName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        var result = new CategoryDto(category.Id, category.Name);
        return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory([FromRoute] int id, [FromBody] UpsertCategoryRequest request)
    {
        var normalizedName = NormalizeName(request.Name);
        if (normalizedName is null)
        {
            return BadRequest(new { message = "Tên danh mục là bắt buộc." });
        }

        var category = await _dbContext.Categories.FirstOrDefaultAsync(item => item.Id == id);
        if (category is null)
        {
            return NotFound(new { message = "Không tìm thấy danh mục." });
        }

        var duplicated = await _dbContext.Categories
            .AsNoTracking()
            .AnyAsync(item => item.Id != id && item.Name.ToLower() == normalizedName.ToLower());

        if (duplicated)
        {
            return BadRequest(new { message = "Tên danh mục đã tồn tại." });
        }

        category.Name = normalizedName;
        category.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(new CategoryDto(category.Id, category.Name));
    }

    [HttpDelete("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory([FromRoute] int id)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(item => item.Id == id);
        if (category is null)
        {
            return NotFound(new { message = "Không tìm thấy danh mục." });
        }

        var hasPlants = await _dbContext.Plants
            .AsNoTracking()
            .AnyAsync(item => item.CategoryId == id && !item.IsDeleted);

        if (hasPlants)
        {
            return BadRequest(new { message = "Không thể xoá danh mục đang có sản phẩm." });
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private static string? NormalizeName(string? input)
    {
        var trimmed = input?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    public sealed class UpsertCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}
