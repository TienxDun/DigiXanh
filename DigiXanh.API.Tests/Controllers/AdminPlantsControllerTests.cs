using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DigiXanh.API.Data;
using DigiXanh.API.DTOs.Common;
using DigiXanh.API.DTOs.Plants;
using DigiXanh.API.Models;
using DigiXanh.API.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DigiXanh.API.Tests.Controllers;

public class AdminPlantsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AdminPlantsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPlants_ReturnsForbidden_WhenUserRoleIsNotAdmin()
    {
        using var client = _factory.CreateClient();
        await ResetAndSeedAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "user-token");

        var response = await client.GetAsync("/api/admin/plants");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetPlants_ReturnsPagedData_ForAdminRole()
    {
        using var client = _factory.CreateClient();
        await ResetAndSeedAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var response = await client.GetAsync("/api/admin/plants?page=2&pageSize=2");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<PagedResult<PlantDto>>();

        Assert.NotNull(payload);
        Assert.Equal(4, payload.TotalCount);
        Assert.Equal(2, payload.Page);
        Assert.Equal(2, payload.PageSize);
        Assert.Equal(2, payload.TotalPages);
        Assert.Equal(2, payload.Items.Count);
    }

    [Fact]
    public async Task GetPlants_FiltersBySearch_AndIgnoresSoftDeleted()
    {
        using var client = _factory.CreateClient();
        await ResetAndSeedAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var response = await client.GetAsync("/api/admin/plants?search=monstera");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<PagedResult<PlantDto>>();

        Assert.NotNull(payload);
        Assert.Equal(1, payload.TotalCount);

        var plant = Assert.Single(payload.Items);
        Assert.Equal("Cây Monstera", plant.Name);
    }

    [Fact]
    public async Task CreatePlant_ReturnsBadRequest_WhenPriceIsInvalid()
    {
        using var client = _factory.CreateClient();
        await ResetAndSeedAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var payload = new
        {
            name = "Cây mới",
            scientificName = "Plantus testus",
            description = "Mô tả",
            price = 0,
            categoryId = 1,
            imageUrl = "https://example.com/new.jpg"
        };

        var response = await client.PostAsJsonAsync("/api/admin/plants", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlant_ReturnsBadRequest_WhenCategoryNotExists()
    {
        using var client = _factory.CreateClient();
        await ResetAndSeedAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var payload = new
        {
            name = "Cây mới",
            scientificName = "Plantus testus",
            description = "Mô tả",
            price = 123000,
            categoryId = 999,
            imageUrl = "https://example.com/new.jpg"
        };

        var response = await client.PostAsJsonAsync("/api/admin/plants", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlant_ReturnsCreated_WhenInputIsValid()
    {
        using var client = _factory.CreateClient();
        await ResetAndSeedAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var payload = new
        {
            name = "Cây mới",
            scientificName = "Plantus testus",
            description = "Mô tả",
            price = 123000,
            categoryId = 1,
            imageUrl = "https://example.com/new.jpg"
        };

        var response = await client.PostAsJsonAsync("/api/admin/plants", payload);
        response.EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<PlantDto>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("Cây mới", created.Name);
        Assert.Equal(123000, created.Price);
    }

    [Fact]
    public async Task GetPlantById_ReturnsPlantDetail_ForAdminRole()
    {
        using var client = _factory.CreateClient();
        await ResetAndSeedAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var response = await client.GetAsync("/api/admin/plants/1");

        response.EnsureSuccessStatusCode();
        var detail = await response.Content.ReadFromJsonAsync<PlantDetailDto>();

        Assert.NotNull(detail);
        Assert.Equal(1, detail.Id);
        Assert.Equal("Cây Monstera", detail.Name);
    }

    [Fact]
    public async Task UpdatePlant_UpdatesData_ForAdminRole()
    {
        using var client = _factory.CreateClient();
        await ResetAndSeedAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var payload = new
        {
            name = "Cây Monstera Update",
            scientificName = "Monstera deliciosa",
            description = "Đã cập nhật",
            price = 555000,
            categoryId = 1,
            imageUrl = "https://example.com/updated.jpg"
        };

        var response = await client.PutAsJsonAsync("/api/admin/plants/1", payload);

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<PlantDto>();

        Assert.NotNull(updated);
        Assert.Equal("Cây Monstera Update", updated.Name);
        Assert.Equal(555000, updated.Price);
    }

    [Fact]
    public async Task SoftDeletePlant_MarksAsDeleted_AndDisappearsFromList()
    {
        using var client = _factory.CreateClient();
        await ResetAndSeedAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var deleteResponse = await client.DeleteAsync("/api/admin/plants/1");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var listResponse = await client.GetAsync("/api/admin/plants");
        listResponse.EnsureSuccessStatusCode();

        var list = await listResponse.Content.ReadFromJsonAsync<PagedResult<PlantDto>>();
        Assert.NotNull(list);
        Assert.DoesNotContain(list.Items, item => item.Id == 1);
    }

    [Fact]
    public async Task BulkSoftDeletePlants_MarksSelectedAsDeleted()
    {
        using var client = _factory.CreateClient();
        await ResetAndSeedAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var bulkDeletePayload = new
        {
            ids = new[] { 1, 2 }
        };

        var deleteResponse = await client.PostAsJsonAsync("/api/admin/plants/bulk-soft-delete", bulkDeletePayload);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var listResponse = await client.GetAsync("/api/admin/plants");
        listResponse.EnsureSuccessStatusCode();

        var list = await listResponse.Content.ReadFromJsonAsync<PagedResult<PlantDto>>();
        Assert.NotNull(list);
        Assert.DoesNotContain(list.Items, item => item.Id == 1);
        Assert.DoesNotContain(list.Items, item => item.Id == 2);
    }

    private async Task ResetAndSeedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        var category = new Category { Name = "Cây nội thất" };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();

        dbContext.Plants.AddRange(
            new Plant
            {
                Name = "Cây Monstera",
                ScientificName = "Monstera deliciosa",
                Price = 450000,
                CategoryId = category.Id,
                ImageUrl = "https://example.com/monstera.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false
            },
            new Plant
            {
                Name = "Cây Lưỡi Hổ",
                ScientificName = "Dracaena trifasciata",
                Price = 220000,
                CategoryId = category.Id,
                ImageUrl = "https://example.com/luoi-ho.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                IsDeleted = false
            },
            new Plant
            {
                Name = "Cây Trầu Bà",
                ScientificName = "Epipremnum aureum",
                Price = 180000,
                CategoryId = category.Id,
                ImageUrl = "https://example.com/trau-ba.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                IsDeleted = false
            },
            new Plant
            {
                Name = "Cây Kim Ngân",
                ScientificName = "Pachira aquatica",
                Price = 380000,
                CategoryId = category.Id,
                ImageUrl = "https://example.com/kim-ngan.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                IsDeleted = false
            },
            new Plant
            {
                Name = "Cây Monstera Đã Xoá",
                ScientificName = "Monstera obliqua",
                Price = 900000,
                CategoryId = category.Id,
                ImageUrl = "https://example.com/monstera-deleted.jpg",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                IsDeleted = true
            });

        await dbContext.SaveChangesAsync();
    }
}
