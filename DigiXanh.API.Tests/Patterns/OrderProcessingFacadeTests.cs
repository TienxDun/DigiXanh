using DigiXanh.API.Data;
using DigiXanh.API.DTOs.Orders;
using DigiXanh.API.Models;
using DigiXanh.API.Patterns.Adapter;
using DigiXanh.API.Patterns.Facade;
using DigiXanh.API.Services.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DigiXanh.API.Tests.Patterns;

public class OrderProcessingFacadeTests : IDisposable
{
    private const string TestUserId = "test-user-id";
    private const string TestUserEmail = "test@example.com";
    private const string TestUserName = "Test User";

    private SqliteConnection? _connection;

    public void Dispose()
    {
        _connection?.Dispose();
    }

    #region Test: Thành công với thanh toán tiền mặt

    [Fact]
    public async Task PlaceOrderAsync_ReturnsSuccess_WithCashPayment()
    {
        // Arrange
        var (dbContext, connection) = CreateDbContext();
        _connection = connection;
        await SeedUserAndCartAsync(dbContext);
        
        var mockPaymentFactory = new Mock<IPaymentAdapterFactory>();
        var mockEmailService = new Mock<IOrderEmailService>();
        
        mockPaymentFactory
            .Setup(f => f.GetAdapter(PaymentMethod.Cash))
            .Returns(new FakeCashPaymentAdapter());

        var facade = CreateFacade(dbContext, mockPaymentFactory.Object, mockEmailService.Object);
        var request = CreateValidOrderRequest(PaymentMethodDto.Cash);

        // Act
        var result = await facade.PlaceOrderAsync(TestUserId, request, "127.0.0.1");

        // Assert
        Assert.True(result.Success);
        Assert.True(result.OrderId > 0);
        Assert.Null(result.PaymentUrl); // Cash không có payment URL
        Assert.NotNull(result.Order);
        Assert.Equal("Đơn hàng đã được đặt thành công. Bạn sẽ thanh toán khi nhận hàng.", result.Message);

        // Kiểm tra order được tạo trong DB
        var order = await dbContext.Orders
            .Include(o => o.OrderItems)
            .FirstAsync(o => o.Id == result.OrderId);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(PaymentMethod.Cash, order.PaymentMethod);
        Assert.Equal(2, order.OrderItems.Count);

        // Kiểm tra giỏ hàng đã bị xóa
        var cartItems = await dbContext.CartItems.Where(ci => ci.UserId == TestUserId).ToListAsync();
        Assert.Empty(cartItems);

        // Verify email được gọi
        mockEmailService.Verify(
            e => e.SendOrderConfirmationEmailAsync(TestUserEmail, It.IsAny<Order>(), TestUserName),
            Times.Once);
    }

    #endregion

    #region Test: Thành công với VNPay

    [Fact]
    public async Task PlaceOrderAsync_ReturnsPaymentUrl_WithVNPayPayment()
    {
        // Arrange
        var (dbContext, connection) = CreateDbContext();
        _connection = connection;
        await SeedUserAndCartAsync(dbContext);
        
        var mockPaymentFactory = new Mock<IPaymentAdapterFactory>();
        var mockEmailService = new Mock<IOrderEmailService>();
        
        const string expectedPaymentUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?test=1";
        mockPaymentFactory
            .Setup(f => f.GetAdapter(PaymentMethod.VNPay))
            .Returns(new FakeVNPayPaymentAdapter(expectedPaymentUrl));

        var config = CreateConfiguration();
        var facade = CreateFacade(dbContext, mockPaymentFactory.Object, mockEmailService.Object, config);
        var request = CreateValidOrderRequest(PaymentMethodDto.VNPay);

        // Act
        var result = await facade.PlaceOrderAsync(TestUserId, request, "127.0.0.1");

        // Assert
        Assert.True(result.Success);
        Assert.True(result.OrderId > 0);
        Assert.NotNull(result.PaymentUrl);
        Assert.Equal(expectedPaymentUrl, result.PaymentUrl);
        Assert.NotNull(result.Order);
        Assert.Equal("Chuyển hướng đến cổng thanh toán VNPay", result.Message);

        // Kiểm tra order được tạo với trạng thái Pending
        var order = await dbContext.Orders.FindAsync(result.OrderId);
        Assert.Equal(OrderStatus.Pending, order!.Status);
        Assert.Equal(PaymentMethod.VNPay, order.PaymentMethod);

        // Kiểm tra giỏ hàng đã bị xóa
        var cartItems = await dbContext.CartItems.Where(ci => ci.UserId == TestUserId).ToListAsync();
        Assert.Empty(cartItems);

        // Verify email được gọi
        mockEmailService.Verify(
            e => e.SendOrderConfirmationEmailAsync(TestUserEmail, It.IsAny<Order>(), TestUserName),
            Times.Once);
    }

    #endregion

    #region Test: Thất bại khi giỏ hàng trống

    [Fact]
    public async Task PlaceOrderAsync_ReturnsFailure_WhenCartIsEmpty()
    {
        // Arrange
        var (dbContext, connection) = CreateDbContext();
        _connection = connection;
        
        // Chỉ tạo user, không tạo cart items
        var user = new ApplicationUser
        {
            Id = TestUserId,
            Email = TestUserEmail,
            UserName = TestUserEmail,
            FullName = TestUserName
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var mockPaymentFactory = new Mock<IPaymentAdapterFactory>();
        var mockEmailService = new Mock<IOrderEmailService>();
        
        var facade = CreateFacade(dbContext, mockPaymentFactory.Object, mockEmailService.Object);
        var request = CreateValidOrderRequest(PaymentMethodDto.Cash);

        // Act
        var result = await facade.PlaceOrderAsync(TestUserId, request, "127.0.0.1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(0, result.OrderId);
        Assert.Equal("Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi đặt hàng.", result.Message);

        // Verify không có order nào được tạo
        var orders = await dbContext.Orders.ToListAsync();
        Assert.Empty(orders);

        // Verify không gọi payment adapter
        mockPaymentFactory.Verify(f => f.GetAdapter(It.IsAny<PaymentMethod>()), Times.Never);

        // Verify không gọi email service
        mockEmailService.Verify(
            e => e.SendOrderConfirmationEmailAsync(It.IsAny<string>(), It.IsAny<Order>(), It.IsAny<string>()),
            Times.Never);
    }

    #endregion

    #region Test: Thất bại khi payment adapter throw exception (Transaction Rollback)

    [Fact]
    public async Task PlaceOrderAsync_RollsBackTransaction_WhenPaymentAdapterThrowsException()
    {
        // Arrange
        var (dbContext, connection) = CreateDbContext();
        _connection = connection;
        await SeedUserAndCartAsync(dbContext);
        
        var mockPaymentFactory = new Mock<IPaymentAdapterFactory>();
        var mockEmailService = new Mock<IOrderEmailService>();
        
        mockPaymentFactory
            .Setup(f => f.GetAdapter(PaymentMethod.Cash))
            .Returns(new ExceptionThrowingPaymentAdapter());

        var facade = CreateFacade(dbContext, mockPaymentFactory.Object, mockEmailService.Object);
        var request = CreateValidOrderRequest(PaymentMethodDto.Cash);

        // Act
        var result = await facade.PlaceOrderAsync(TestUserId, request, "127.0.0.1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Có lỗi xảy ra khi xử lý đơn hàng. Vui lòng thử lại sau.", result.Message);

        // Kiểm tra KHÔNG có order nào được tạo (đã rollback)
        var orders = await dbContext.Orders.ToListAsync();
        Assert.Empty(orders);

        // Kiểm tra giỏ hàng VẪN CÒN (chưa bị xóa vì transaction rollback)
        var cartItems = await dbContext.CartItems.Where(ci => ci.UserId == TestUserId).ToListAsync();
        Assert.Equal(2, cartItems.Count);

        // Verify không gọi email service vì lỗi
        mockEmailService.Verify(
            e => e.SendOrderConfirmationEmailAsync(It.IsAny<string>(), It.IsAny<Order>(), It.IsAny<string>()),
            Times.Never);
    }

    #endregion

    #region Test: Thất bại khi payment adapter trả về lỗi (không phải exception)

    [Fact]
    public async Task PlaceOrderAsync_ReturnsFailure_WhenPaymentAdapterReturnsError()
    {
        // Arrange
        var (dbContext, connection) = CreateDbContext();
        _connection = connection;
        await SeedUserAndCartAsync(dbContext);
        
        var mockPaymentFactory = new Mock<IPaymentAdapterFactory>();
        var mockEmailService = new Mock<IOrderEmailService>();
        
        mockPaymentFactory
            .Setup(f => f.GetAdapter(PaymentMethod.VNPay))
            .Returns(new ErrorPaymentAdapter("Thiếu cấu hình VNPay: TmnCode hoặc HashSecret."));

        var facade = CreateFacade(dbContext, mockPaymentFactory.Object, mockEmailService.Object);
        var request = CreateValidOrderRequest(PaymentMethodDto.VNPay);

        // Act
        var result = await facade.PlaceOrderAsync(TestUserId, request, "127.0.0.1");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Thiếu cấu hình VNPay: TmnCode hoặc HashSecret.", result.Message);

        // Kiểm tra KHÔNG có order nào được tạo (đã rollback)
        var orders = await dbContext.Orders.ToListAsync();
        Assert.Empty(orders);

        // Kiểm tra giỏ hàng VẪN CÒN (chưa bị xóa vì transaction rollback)
        var cartItems = await dbContext.CartItems.Where(ci => ci.UserId == TestUserId).ToListAsync();
        Assert.Equal(2, cartItems.Count);

        // Verify không gọi email service vì thanh toán lỗi
        mockEmailService.Verify(
            e => e.SendOrderConfirmationEmailAsync(It.IsAny<string>(), It.IsAny<Order>(), It.IsAny<string>()),
            Times.Never);
    }

    #endregion

    #region Test: Validation thông tin giao hàng

    [Theory]
    [InlineData("", "0900000000", "123 ABC Street", "Vui lòng nhập tên ngườI nhận.")]
    [InlineData("Test", "", "123 ABC Street", "Vui lòng nhập số điện thoại.")]
    [InlineData("Test", "0900000000", "", "Vui lòng nhập địa chỉ giao hàng.")]
    [InlineData("T", "0900000000", "123 ABC Street", "Tên ngườI nhận phải từ 2 đến 200 ký tự.")]
    [InlineData("Test", "123", "123 ABC Street", "Số điện thoại không hợp lệ")]
    [InlineData("Test", "0900000000", "Short", "Địa chỉ giao hàng phải từ 10 đến 500 ký tự.")]
    public async Task PlaceOrderAsync_ReturnsValidationError_WhenShippingInfoInvalid(
        string recipientName, string phone, string address, string expectedError)
    {
        // Arrange
        var (dbContext, connection) = CreateDbContext();
        _connection = connection;
        await SeedUserAndCartAsync(dbContext);
        
        var mockPaymentFactory = new Mock<IPaymentAdapterFactory>();
        var mockEmailService = new Mock<IOrderEmailService>();
        
        var facade = CreateFacade(dbContext, mockPaymentFactory.Object, mockEmailService.Object);
        var request = new CreateOrderRequest
        {
            RecipientName = recipientName,
            Phone = phone,
            ShippingAddress = address,
            PaymentMethod = PaymentMethodDto.Cash
        };

        // Act
        var result = await facade.PlaceOrderAsync(TestUserId, request, "127.0.0.1");

        // Assert
        Assert.False(result.Success);
        Assert.Contains(expectedError, result.Message);

        // Verify không có order nào được tạo
        var orders = await dbContext.Orders.ToListAsync();
        Assert.Empty(orders);
    }

    #endregion

    #region Test: Giảm giá được áp dụng đúng

    [Fact]
    public async Task PlaceOrderAsync_AppliesDiscountCorrectly()
    {
        // Arrange - Tạo giỏ hàng với 3 items (>=3 giảm 7%)
        var (dbContext, connection) = CreateDbContext();
        _connection = connection;
        await SeedUserAndCartWithQuantitiesAsync(dbContext, 1, 2); // Tổng 3 items
        
        var mockPaymentFactory = new Mock<IPaymentAdapterFactory>();
        var mockEmailService = new Mock<IOrderEmailService>();
        
        mockPaymentFactory
            .Setup(f => f.GetAdapter(PaymentMethod.Cash))
            .Returns(new FakeCashPaymentAdapter());

        var facade = CreateFacade(dbContext, mockPaymentFactory.Object, mockEmailService.Object);
        var request = CreateValidOrderRequest(PaymentMethodDto.Cash);

        // Act
        var result = await facade.PlaceOrderAsync(TestUserId, request, "127.0.0.1");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Order);
        
        // Giá gốc: 100000 + 2*80000 = 260000
        // Giảm 7%: 260000 * 0.07 = 18200
        // Final: 241800
        Assert.Equal(260000m, result.Order!.TotalAmount);
        Assert.Equal(18200m, result.Order.DiscountAmount);
        Assert.Equal(241800m, result.Order.FinalAmount);
    }

    #endregion

    #region Helper Methods

    private static (ApplicationDbContext Context, SqliteConnection Connection) CreateDbContext()
    {
        // Sử dụng SQLite In-Memory mode để hỗ trợ transaction
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        return (context, connection);
    }

    private static async Task SeedUserAndCartAsync(ApplicationDbContext dbContext)
    {
        // Seed User
        var user = new ApplicationUser
        {
            Id = TestUserId,
            Email = TestUserEmail,
            UserName = TestUserEmail,
            FullName = TestUserName
        };
        dbContext.Users.Add(user);

        // Seed Categories
        var category = new Category { Id = 1, Name = "Cây để bàn" };
        dbContext.Categories.Add(category);

        // Seed Plants
        var plant1 = new Plant 
        { 
            Id = 1, 
            Name = "Cây lưỡi hổ", 
            ScientificName = "Sansevieria",
            Price = 100000m, 
            CategoryId = 1,
            IsDeleted = false
        };
        var plant2 = new Plant 
        { 
            Id = 2, 
            Name = "Cây kim tiền", 
            ScientificName = "Zamioculcas",
            Price = 80000m, 
            CategoryId = 1,
            IsDeleted = false
        };
        dbContext.Plants.AddRange(plant1, plant2);

        // Seed Cart Items
        dbContext.CartItems.AddRange(
            new CartItem { UserId = TestUserId, PlantId = 1, Quantity = 1 },
            new CartItem { UserId = TestUserId, PlantId = 2, Quantity = 1 }
        );

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedUserAndCartWithQuantitiesAsync(
        ApplicationDbContext dbContext, int qty1, int qty2)
    {
        var user = new ApplicationUser
        {
            Id = TestUserId,
            Email = TestUserEmail,
            UserName = TestUserEmail,
            FullName = TestUserName
        };
        dbContext.Users.Add(user);

        var category = new Category { Id = 1, Name = "Cây để bàn" };
        dbContext.Categories.Add(category);

        var plant1 = new Plant 
        { 
            Id = 1, 
            Name = "Cây lưỡi hổ", 
            ScientificName = "Sansevieria",
            Price = 100000m, 
            CategoryId = 1,
            IsDeleted = false
        };
        var plant2 = new Plant 
        { 
            Id = 2, 
            Name = "Cây kim tiền", 
            ScientificName = "Zamioculcas",
            Price = 80000m, 
            CategoryId = 1,
            IsDeleted = false
        };
        dbContext.Plants.AddRange(plant1, plant2);

        dbContext.CartItems.AddRange(
            new CartItem { UserId = TestUserId, PlantId = 1, Quantity = qty1 },
            new CartItem { UserId = TestUserId, PlantId = 2, Quantity = qty2 }
        );

        await dbContext.SaveChangesAsync();
    }

    private static CreateOrderRequest CreateValidOrderRequest(PaymentMethodDto method)
    {
        return new CreateOrderRequest
        {
            RecipientName = "Nguyễn Văn A",
            Phone = "0900000000",
            ShippingAddress = "123 Lê Lợi, Q.1, TP.HCM",
            PaymentMethod = method,
            ReturnUrl = "https://example.com/payment-return"
        };
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VNPay:TmnCode"] = "TESTCODE",
                ["VNPay:HashSecret"] = "TESTSECRET",
                ["VNPay:Url"] = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"
            })
            .Build();
    }

    private static OrderProcessingFacade CreateFacade(
        ApplicationDbContext dbContext,
        IPaymentAdapterFactory paymentFactory,
        IOrderEmailService emailService,
        IConfiguration? config = null)
    {
        return new OrderProcessingFacade(
            dbContext,
            paymentFactory,
            emailService,
            NullLogger<OrderProcessingFacade>.Instance,
            config ?? CreateConfiguration());
    }

    #endregion

    #region Fake Adapters

    private class FakeCashPaymentAdapter : IPaymentAdapter
    {
        public Task<PaymentResult> ProcessPaymentAsync(Order order, PaymentInfo paymentInfo)
        {
            order.Status = OrderStatus.Pending;
            order.PaymentMethod = PaymentMethod.Cash;
            
            return Task.FromResult(new PaymentResult
            {
                Success = true,
                Message = "Đơn hàng đã được đặt thành công. Bạn sẽ thanh toán khi nhận hàng.",
                TransactionId = null,
                PaymentUrl = null
            });
        }
    }

    private class FakeVNPayPaymentAdapter : IPaymentAdapter
    {
        private readonly string _paymentUrl;

        public FakeVNPayPaymentAdapter(string paymentUrl)
        {
            _paymentUrl = paymentUrl;
        }

        public Task<PaymentResult> ProcessPaymentAsync(Order order, PaymentInfo paymentInfo)
        {
            order.Status = OrderStatus.Pending;
            order.PaymentMethod = PaymentMethod.VNPay;
            
            return Task.FromResult(new PaymentResult
            {
                Success = true,
                Message = "Chuyển hướng đến cổng thanh toán VNPay",
                TransactionId = null,
                PaymentUrl = _paymentUrl
            });
        }
    }

    private class ExceptionThrowingPaymentAdapter : IPaymentAdapter
    {
        public Task<PaymentResult> ProcessPaymentAsync(Order order, PaymentInfo paymentInfo)
        {
            throw new InvalidOperationException("Simulated payment processing error");
        }
    }

    private class ErrorPaymentAdapter : IPaymentAdapter
    {
        private readonly string _errorMessage;

        public ErrorPaymentAdapter(string errorMessage)
        {
            _errorMessage = errorMessage;
        }

        public Task<PaymentResult> ProcessPaymentAsync(Order order, PaymentInfo paymentInfo)
        {
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                Message = _errorMessage,
                TransactionId = null,
                PaymentUrl = null
            });
        }
    }

    #endregion
}
