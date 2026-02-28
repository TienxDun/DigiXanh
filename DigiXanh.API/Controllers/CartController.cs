using DigiXanh.API.Data;
using DigiXanh.API.DTOs.Cart;
using DigiXanh.API.Helpers;
using DigiXanh.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiXanh.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : BaseController
{
    private const int MaxQuantityPerItem = 99;
    private readonly ApplicationDbContext _dbContext;

    public CartController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CartSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCart()
    {
        var validationResult = await ValidateUserAsync();
        if (validationResult != null) return validationResult;

        var userId = GetCurrentUserId()!;
        var items = await GetCartItemsAsync(userId);
        var summary = CalculateCartSummary(items);

        return Ok(summary);
    }

    [HttpPost("items")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CartSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request)
    {
        if (!ValidateAddItemRequest(request, out var errorResult))
            return errorResult!;

        var validationResult = await ValidateUserAsync();
        if (validationResult != null) return validationResult;

        var userId = GetCurrentUserId()!;
        var plant = await GetPlantAsync(request.PlantId);
        if (plant is null)
            return NotFound(new { message = "Không tìm thấy cây." });

        // Validate stock quantity (US26)
        if (plant.StockQuantity.HasValue)
        {
            if (plant.StockQuantity.Value == 0)
            {
                return BadRequest(new { message = $"Sản phẩm '{plant.Name}' đã hết hàng." });
            }

            // Check existing quantity in cart
            var existingItem = await _dbContext.CartItems
                .FirstOrDefaultAsync(item => item.UserId == userId && item.PlantId == request.PlantId);
            var currentCartQuantity = existingItem?.Quantity ?? 0;
            var totalQuantity = currentCartQuantity + request.Quantity;

            if (totalQuantity > plant.StockQuantity.Value)
            {
                var availableQuantity = plant.StockQuantity.Value - currentCartQuantity;
                if (availableQuantity <= 0)
                {
                    return BadRequest(new { message = $"Bạn đã có {currentCartQuantity} sản phẩm '{plant.Name}' trong giỏ. Không thể thêm thêm do vượt quá tồn kho ({plant.StockQuantity.Value})." });
                }
                return BadRequest(new { message = $"Sản phẩm '{plant.Name}' chỉ còn {plant.StockQuantity.Value} trong kho. Bạn đã có {currentCartQuantity} trong giỏ, chỉ có thể thêm tối đa {availableQuantity} sản phẩm nữa." });
            }
        }

        await AddOrUpdateCartItemAsync(userId, request.PlantId, request.Quantity, plant.StockQuantity);
        await _dbContext.SaveChangesAsync();

        return await GetMyCart();
    }

    [HttpPut("items/{cartItemId:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CartSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, [FromBody] UpdateCartItemQuantityRequest request)
    {
        if (!ValidateUpdateRequest(cartItemId, request.Quantity, out var errorResult))
            return errorResult!;

        var validationResult = await ValidateUserAsync();
        if (validationResult != null) return validationResult;

        var userId = GetCurrentUserId()!;
        var item = await GetCartItemAsync(cartItemId, userId);
        if (item is null)
            return NotFound(new { message = "Không tìm thấy sản phẩm trong giỏ hàng." });

        // Validate stock quantity (US26)
        if (item.Plant?.StockQuantity.HasValue == true)
        {
            if (request.Quantity > item.Plant.StockQuantity.Value)
            {
                return BadRequest(new 
                { 
                    message = $"Sản phẩm '{item.Plant.Name}' chỉ còn {item.Plant.StockQuantity.Value} trong kho. Không thể cập nhật số lượng vượt quá tồn kho." 
                });
            }
        }

        item.Quantity = request.Quantity;
        item.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return await GetMyCart();
    }

    [HttpDelete("items/{cartItemId:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CartSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        if (cartItemId <= 0)
            return BadRequest(new { message = "Cart item không hợp lệ." });

        var validationResult = await ValidateUserAsync();
        if (validationResult != null) return validationResult;

        var userId = GetCurrentUserId()!;
        var item = await _dbContext.CartItems
            .FirstOrDefaultAsync(cartItem => cartItem.Id == cartItemId && cartItem.UserId == userId);

        if (item is null)
            return NotFound(new { message = "Không tìm thấy sản phẩm trong giỏ hàng." });

        _dbContext.CartItems.Remove(item);
        await _dbContext.SaveChangesAsync();

        return await GetMyCart();
    }

    #region Private Helpers

    private async Task<IActionResult?> ValidateUserAsync()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
            return UserNotFoundResult();

        if (!await ValidateUserExistsAsync(_dbContext, userId))
            return UserSessionInvalidResult();

        return null;
    }

    private static bool ValidateAddItemRequest(AddCartItemRequest request, out IActionResult? errorResult)
    {
        errorResult = null;
        
        if (request.PlantId <= 0)
        {
            errorResult = new BadRequestObjectResult(new { message = "PlantId không hợp lệ." });
            return false;
        }

        if (request.Quantity < 1 || request.Quantity > MaxQuantityPerItem)
        {
            errorResult = new BadRequestObjectResult(new { message = $"Số lượng phải từ 1 đến {MaxQuantityPerItem}." });
            return false;
        }

        return true;
    }

    private static bool ValidateUpdateRequest(int cartItemId, int quantity, out IActionResult? errorResult)
    {
        errorResult = null;

        if (cartItemId <= 0)
        {
            errorResult = new BadRequestObjectResult(new { message = "Cart item không hợp lệ." });
            return false;
        }

        if (quantity < 1 || quantity > MaxQuantityPerItem)
        {
            errorResult = new BadRequestObjectResult(new { message = $"Số lượng phải từ 1 đến {MaxQuantityPerItem}." });
            return false;
        }

        return true;
    }

    private async Task<Plant?> GetPlantAsync(int plantId)
    {
        return await _dbContext.Plants
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == plantId && !item.IsDeleted);
    }

    private async Task<List<CartItemDto>> GetCartItemsAsync(string userId)
    {
        return await _dbContext.CartItems
            .AsNoTracking()
            .Include(item => item.Plant)
            .Where(item => item.UserId == userId && item.Plant != null && !item.Plant.IsDeleted)
            .OrderByDescending(item => item.UpdatedAt)
            .Select(item => new CartItemDto(
                item.Id,
                item.PlantId,
                item.Plant!.Name,
                item.Plant.ScientificName,
                item.Plant.Price,
                ImageUrlSanitizer.NormalizeOrEmpty(item.Plant.ImageUrl),
                item.Quantity,
                item.Quantity * item.Plant.Price,
                item.Plant.StockQuantity))
            .ToListAsync();
    }

    private CartSummaryDto CalculateCartSummary(List<CartItemDto> items)
    {
        var totalQuantity = items.Sum(item => item.Quantity);
        var baseAmount = items.Sum(item => item.LineTotal);
        
        var discountPercent = totalQuantity >= 3 ? 7 : totalQuantity >= 2 ? 5 : 0;
        var discountAmount = baseAmount * discountPercent / 100;
        var finalAmount = baseAmount - discountAmount;

        return new CartSummaryDto(items, totalQuantity, baseAmount, discountAmount, discountPercent, finalAmount);
    }

    private async Task AddOrUpdateCartItemAsync(string userId, int plantId, int quantity, int? stockQuantity)
    {
        var existingItem = await _dbContext.CartItems
            .FirstOrDefaultAsync(item => item.UserId == userId && item.PlantId == plantId);

        if (existingItem is null)
        {
            _dbContext.CartItems.Add(new CartItem
            {
                UserId = userId,
                PlantId = plantId,
                Quantity = quantity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            var newQuantity = existingItem.Quantity + quantity;
            // Respect stock limit if available
            if (stockQuantity.HasValue)
            {
                newQuantity = Math.Min(newQuantity, stockQuantity.Value);
            }
            existingItem.Quantity = Math.Min(MaxQuantityPerItem, newQuantity);
            existingItem.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task<CartItem?> GetCartItemAsync(int cartItemId, string userId)
    {
        return await _dbContext.CartItems
            .Include(cartItem => cartItem.Plant)
            .FirstOrDefaultAsync(cartItem => cartItem.Id == cartItemId && cartItem.UserId == userId 
                && cartItem.Plant != null && !cartItem.Plant.IsDeleted);
    }

    #endregion
}
