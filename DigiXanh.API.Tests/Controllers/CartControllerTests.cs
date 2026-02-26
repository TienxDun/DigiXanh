using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DigiXanh.API.Data;
using DigiXanh.API.DTOs.Cart;
using DigiXanh.API.Models;
using DigiXanh.API.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DigiXanh.API.Tests.Controllers;

public class CartControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CartControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMyCart_ReturnsUnauthorized_WhenNoToken()
    {
        using var client = _factory.CreateClient();
        await ResetAndSeedAsync();

        var response = await client.GetAsync("/api/cart");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMyCart_ReturnsCurrentUserItems_AndComputedTotals()
    {
        using var client = _factory.CreateClient();
        await ResetAndSeedAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "user-token");

        var response = await client.GetAsync("/api/cart");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<CartSummaryDto>();

        Assert.NotNull(payload);
        Assert.Equal(2, payload.Items.Count);
        Assert.Equal(3, payload.TotalQuantity);
        Assert.Equal(940000m, payload.TotalAmount);
    }

    [Fact]
    public async Task UpdateQuantity_UpdatesItem_AndRecalculatesTotals()
    {
        using var client = _factory.CreateClient();
        var seed = await ResetAndSeedAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "user-token");

        var response = await client.PutAsJsonAsync($"/api/cart/items/{seed.TargetCartItemId}", new { quantity = 4 });

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<CartSummaryDto>();

        Assert.NotNull(payload);
        Assert.Equal(5, payload.TotalQuantity);
        Assert.Equal(1660000m, payload.TotalAmount);
        Assert.Contains(payload.Items, item => item.Id == seed.TargetCartItemId && item.Quantity == 4);
    }

    [Fact]
    public async Task RemoveItem_RemovesItem_AndReturnsUpdatedCart()
    {
        using var client = _factory.CreateClient();
        var seed = await ResetAndSeedAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "user-token");

        var response = await client.DeleteAsync($"/api/cart/items/{seed.TargetCartItemId}");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<CartSummaryDto>();

        Assert.NotNull(payload);
        Assert.Single(payload.Items);
        Assert.Equal(1, payload.TotalQuantity);
        Assert.Equal(220000m, payload.TotalAmount);
    }

    [Fact]
    public async Task UpdateQuantity_ReturnsBadRequest_WhenQuantityBelowMinimum()
    {
        using var client = _factory.CreateClient();
        var seed = await ResetAndSeedAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "user-token");

        var response = await client.PutAsJsonAsync($"/api/cart/items/{seed.TargetCartItemId}", new { quantity = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<(int TargetCartItemId, int OtherUserCartItemId)> ResetAndSeedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        var user = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "test@digixanh.com",
            Email = "test@digixanh.com",
            FullName = "Test User"
        };

        var anotherUser = new ApplicationUser
        {
            Id = "another-user-id",
            UserName = "other@digixanh.com",
            Email = "other@digixanh.com",
            FullName = "Other User"
        };

        dbContext.Users.AddRange(user, anotherUser);

        var category = new Category { Name = "Cây nội thất" };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();

        var plant1 = new Plant
        {
            Name = "Cây Monstera",
            ScientificName = "Monstera deliciosa",
            Price = 360000m,
            CategoryId = category.Id,
            ImageUrl = "https://example.com/monstera.jpg",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var plant2 = new Plant
        {
            Name = "Cây Lưỡi Hổ",
            ScientificName = "Dracaena trifasciata",
            Price = 220000m,
            CategoryId = category.Id,
            ImageUrl = "https://example.com/luoi-ho.jpg",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var deletedPlant = new Plant
        {
            Name = "Cây đã xóa",
            ScientificName = "Plantus removed",
            Price = 999999m,
            CategoryId = category.Id,
            ImageUrl = "https://example.com/deleted.jpg",
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        dbContext.Plants.AddRange(plant1, plant2, deletedPlant);
        await dbContext.SaveChangesAsync();

        var targetItem = new CartItem
        {
            UserId = "test-user-id",
            PlantId = plant1.Id,
            Quantity = 2,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var secondItem = new CartItem
        {
            UserId = "test-user-id",
            PlantId = plant2.Id,
            Quantity = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var otherUserItem = new CartItem
        {
            UserId = "another-user-id",
            PlantId = plant2.Id,
            Quantity = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var deletedPlantItem = new CartItem
        {
            UserId = "test-user-id",
            PlantId = deletedPlant.Id,
            Quantity = 5,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.CartItems.AddRange(targetItem, secondItem, otherUserItem, deletedPlantItem);
        await dbContext.SaveChangesAsync();

        return (targetItem.Id, otherUserItem.Id);
    }
}
