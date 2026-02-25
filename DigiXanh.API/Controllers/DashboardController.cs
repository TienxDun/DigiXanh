using DigiXanh.API.Constants;
using DigiXanh.API.Data;
using DigiXanh.API.DTOs.Dashboard;
using DigiXanh.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiXanh.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private static readonly OrderStatus[] RevenueStatuses = [OrderStatus.Paid, OrderStatus.Delivered];
    private readonly ApplicationDbContext _dbContext;

    public DashboardController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("admin")]
    [Authorize(Roles = DefaultRoles.Admin)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AdminDashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminDashboard()
    {
        var today = DateTime.UtcNow.Date;
        var fromDate = today.AddDays(-6);

        var ordersQuery = _dbContext.Orders.AsNoTracking();

        var totalOrders = await ordersQuery.CountAsync();

        var totalRevenue = await ordersQuery
            .Where(order => RevenueStatuses.Contains(order.Status))
            .Select(order => (decimal?)order.TotalAmount)
            .SumAsync() ?? 0m;

        var todayOrders = await ordersQuery
            .Where(order => order.OrderDate >= today && order.OrderDate < today.AddDays(1))
            .CountAsync();

        var todayRevenue = await ordersQuery
            .Where(order =>
                RevenueStatuses.Contains(order.Status) &&
                order.OrderDate >= today &&
                order.OrderDate < today.AddDays(1))
            .Select(order => (decimal?)order.TotalAmount)
            .SumAsync() ?? 0m;

        var groupedDailyOrders = await ordersQuery
            .Where(order => order.OrderDate >= fromDate && order.OrderDate < today.AddDays(1))
            .GroupBy(order => order.OrderDate.Date)
            .Select(group => new
            {
                Date = group.Key,
                Count = group.Count()
            })
            .ToListAsync();

        var dailyOrders = Enumerable.Range(0, 7)
            .Select(offset => fromDate.AddDays(offset))
            .Select(date => new DailyOrderStatDto(
                DateOnly.FromDateTime(date),
                date.ToString("dd/MM"),
                groupedDailyOrders.FirstOrDefault(item => item.Date == date)?.Count ?? 0))
            .ToList();

        return Ok(new AdminDashboardDto(
            totalOrders,
            totalRevenue,
            todayOrders,
            todayRevenue,
            dailyOrders,
            DateTime.UtcNow));
    }
}
