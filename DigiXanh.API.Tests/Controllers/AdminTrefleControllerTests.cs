using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DigiXanh.API.DTOs.Trefle;
using DigiXanh.API.Services.Implementations;
using DigiXanh.API.Services.Interfaces;
using DigiXanh.API.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DigiXanh.API.Tests.Controllers;

public class AdminTrefleControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AdminTrefleControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Search_ReturnsBadRequest_WhenQueryIsMissing()
    {
        using var client = CreateClientWithFakeService(new FakeTrefleService());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var response = await client.GetAsync("/api/admin/trefle/search");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_ReturnsMappedItems_WhenServiceSucceeds()
    {
        using var client = CreateClientWithFakeService(new FakeTrefleService());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var response = await client.GetAsync("/api/admin/trefle/search?q=monstera");
        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<List<TreflePlantSearchItemDto>>();
        Assert.NotNull(items);
        Assert.Single(items);
        Assert.Equal(123, items[0].Id);
        Assert.Equal("Monstera deliciosa", items[0].ScientificName);
    }

    [Fact]
    public async Task Search_ReturnsGatewayTimeout_WhenServiceTimesOut()
    {
        using var client = CreateClientWithFakeService(new FakeTrefleService { ThrowTimeout = true });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var response = await client.GetAsync("/api/admin/trefle/search?q=monstera");

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsTooManyRequests_WhenRateLimitExceeded()
    {
        using var client = CreateClientWithFakeService(new FakeTrefleService { ThrowRateLimit = true });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var response = await client.GetAsync("/api/admin/trefle/123");

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenPlantNotFound()
    {
        using var client = CreateClientWithFakeService(new FakeTrefleService { ReturnNullDetail = true });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var response = await client.GetAsync("/api/admin/trefle/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private HttpClient CreateClientWithFakeService(FakeTrefleService fakeService)
    {
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(ITrefleService));
                services.AddSingleton<ITrefleService>(fakeService);
            });
        });

        return factory.CreateClient();
    }

    private class FakeTrefleService : ITrefleService
    {
        public bool ThrowTimeout { get; init; }
        public bool ThrowRateLimit { get; init; }
        public bool ReturnNullDetail { get; init; }

        public Task<IReadOnlyCollection<TreflePlantSearchItemDto>> SearchPlantsAsync(string query, CancellationToken cancellationToken = default)
        {
            if (ThrowTimeout)
            {
                throw new TrefleTimeoutException("timeout");
            }

            if (ThrowRateLimit)
            {
                throw new TrefleRateLimitException("limit");
            }

            IReadOnlyCollection<TreflePlantSearchItemDto> result =
            [
                new(123, "Trầu bà lá xẻ", "Monstera deliciosa", "https://example.com/monstera.jpg")
            ];

            return Task.FromResult(result);
        }

        public Task<TreflePlantDetailDto?> GetPlantDetailAsync(int id, CancellationToken cancellationToken = default)
        {
            if (ThrowTimeout)
            {
                throw new TrefleTimeoutException("timeout");
            }

            if (ThrowRateLimit)
            {
                throw new TrefleRateLimitException("limit");
            }

            if (ReturnNullDetail)
            {
                return Task.FromResult<TreflePlantDetailDto?>(null);
            }

            return Task.FromResult<TreflePlantDetailDto?>(
                new TreflePlantDetailDto(
                    id,
                    "Trầu bà lá xẻ",
                    "Monstera deliciosa",
                    "Mô tả",
                    "https://example.com/monstera.jpg",
                    "Araceae",
                    "Monstera",
                    "Refs",
                    "Author",
                    2020));
        }
    }
}
