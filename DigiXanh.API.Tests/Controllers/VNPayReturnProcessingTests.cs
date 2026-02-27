using System.Net;
using System.Security.Cryptography;
using System.Text;
using DigiXanh.API.Data;
using DigiXanh.API.Models;
using DigiXanh.API.Patterns.Adapter;
using DigiXanh.API.Patterns.Facade;
using DigiXanh.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DigiXanh.API.Tests.Controllers;

public class VNPayReturnProcessingTests
{
    private const string HashSecret = "test-secret-key";

    [Fact]
    public async Task ProcessVNPayReturnAsync_UpdatesOrderToPaid_WhenResponseIsSuccess()
    {
        await using var dbContext = CreateDbContext();
        var order = await SeedPendingOrderAsync(dbContext);
        var facade = CreateFacade(dbContext);

        var payload = BuildSignedPayload(order.Id, "00", "00", "TXN-0001");
        var result = await facade.ProcessVNPayReturnAsync(payload);

        var updatedOrder = await dbContext.Orders.FirstAsync(o => o.Id == order.Id);
        Assert.True(result.Success);
        Assert.Equal(OrderStatus.Paid, updatedOrder.Status);
        Assert.Equal("TXN-0001", updatedOrder.TransactionId);
    }

    [Fact]
    public async Task ProcessVNPayReturnAsync_UpdatesOrderToCancelled_WhenResponseIsFailure()
    {
        await using var dbContext = CreateDbContext();
        var order = await SeedPendingOrderAsync(dbContext);
        var facade = CreateFacade(dbContext);

        var payload = BuildSignedPayload(order.Id, "24", "02", "TXN-0002");
        var result = await facade.ProcessVNPayReturnAsync(payload);

        var updatedOrder = await dbContext.Orders.FirstAsync(o => o.Id == order.Id);
        Assert.False(result.Success);
        Assert.Equal("Cancelled", result.Status);
        Assert.Equal(OrderStatus.Cancelled, updatedOrder.Status);
        Assert.Equal("TXN-0002", updatedOrder.TransactionId);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"VNPayReturnTests-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<Order> SeedPendingOrderAsync(ApplicationDbContext dbContext)
    {
        // Seed user trước để tránh lỗi FK
        var user = new ApplicationUser
        {
            Id = "test-user-id",
            Email = "test@example.com",
            UserName = "test@example.com",
            FullName = "Test User"
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var order = new Order
        {
            UserId = "test-user-id",
            OrderDate = DateTime.UtcNow,
            TotalAmount = 100000m,
            DiscountAmount = 0,
            FinalAmount = 100000m,
            Status = OrderStatus.Pending,
            RecipientName = "Test User",
            Phone = "0900000000",
            ShippingAddress = "Ho Chi Minh",
            PaymentMethod = PaymentMethod.VNPay
        };

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();
        return order;
    }

    private static OrderProcessingFacade CreateFacade(ApplicationDbContext dbContext)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VNPay:HashSecret"] = HashSecret
            })
            .Build();

        var mockEmailService = new Mock<IOrderEmailService>();
        
        return new OrderProcessingFacade(
            dbContext,
            new FakePaymentAdapterFactory(),
            mockEmailService.Object,
            NullLogger<OrderProcessingFacade>.Instance,
            config);
    }

    private static Dictionary<string, string> BuildSignedPayload(int orderId, string responseCode, string transactionStatus, string transactionNo)
    {
        var payload = new Dictionary<string, string>
        {
            ["vnp_TxnRef"] = orderId.ToString(),
            ["vnp_ResponseCode"] = responseCode,
            ["vnp_TransactionStatus"] = transactionStatus,
            ["vnp_TransactionNo"] = transactionNo,
            ["vnp_Amount"] = "10000000"
        };

        payload["vnp_SecureHash"] = ComputeSecureHash(payload, HashSecret);
        return payload;
    }

    private static string ComputeSecureHash(Dictionary<string, string> payload, string hashSecret)
    {
        var sortedData = new SortedList<string, string>();
        foreach (var item in payload)
        {
            if (!item.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (item.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase)
                || item.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(item.Value))
            {
                sortedData[item.Key] = item.Value;
            }
        }

        var query = string.Join("&", sortedData.Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));
        var secretBytes = Encoding.UTF8.GetBytes(hashSecret);
        var queryBytes = Encoding.UTF8.GetBytes(query);

        using var hmac = new HMACSHA512(secretBytes);
        var hashBytes = hmac.ComputeHash(queryBytes);
        return Convert.ToHexString(hashBytes);
    }

    private sealed class FakePaymentAdapterFactory : IPaymentAdapterFactory
    {
        public IPaymentAdapter GetAdapter(PaymentMethod method)
        {
            throw new NotSupportedException("Not used in VNPay return tests.");
        }
    }
}
