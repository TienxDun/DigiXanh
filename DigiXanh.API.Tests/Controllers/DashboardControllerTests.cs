using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DigiXanh.API.Data;
using DigiXanh.API.DTOs.Dashboard;
using DigiXanh.API.Models;
using DigiXanh.API.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DigiXanh.API.Tests.Controllers;

public class DashboardControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DashboardControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAdminDashboard_ReturnsForbidden_WhenUserRoleIsNotAdmin()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "user-token");

        var response = await client.GetAsync("/api/dashboard/admin");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAdminDashboard_ReturnsAggregatedMetrics_ForAdminRole()
    {
        using var client = _factory.CreateClient();
        await ResetAndSeedAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var response = await client.GetAsync("/api/dashboard/admin");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<AdminDashboardDto>();

        Assert.NotNull(payload);
        Assert.Equal(5, payload.TotalOrders);
        Assert.Equal(1_000_000m, payload.TotalRevenue);
        Assert.Equal(2, payload.TodayOrders);
        Assert.Equal(500_000m, payload.TodayRevenue);
        Assert.Equal(7, payload.DailyOrders.Count);

        var todayLabel = DateTime.UtcNow.Date.ToString("dd/MM");
        var todayStat = Assert.Single(payload.DailyOrders, item => item.Label == todayLabel);
        Assert.Equal(2, todayStat.OrdersCount);
    }

    private async Task ResetAndSeedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        var today = DateTime.UtcNow.Date;

        dbContext.Orders.AddRange(
            new Order
            {
                TotalAmount = 500_000m,
                Status = OrderStatus.Paid,
                OrderDate = today.AddHours(9)
            },
            new Order
            {
                TotalAmount = 250_000m,
                Status = OrderStatus.Delivered,
                OrderDate = today.AddDays(-1).AddHours(8)
            },
            new Order
            {
                TotalAmount = 300_000m,
                Status = OrderStatus.Pending,
                OrderDate = today.AddHours(11)
            },
            new Order
            {
                TotalAmount = 150_000m,
                Status = OrderStatus.Cancelled,
                OrderDate = today.AddDays(-2).AddHours(7)
            },
            new Order
            {
                TotalAmount = 250_000m,
                Status = OrderStatus.Delivered,
                OrderDate = today.AddDays(-8).AddHours(10)
            });

        await dbContext.SaveChangesAsync();
    }
}
