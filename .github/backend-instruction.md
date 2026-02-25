Dưới đây là file **backend-instruction.md** chi tiết dành cho Backend Agent, dựa trên các quyết định đã thống nhất trong dự án DigiXanh và file context.md.

---

# ⚙️ DigiXanh – Backend Instruction (Dành cho BE Agent)

## 1. Vai trò và trách nhiệm
- Xây dựng **ASP.NET Core Web API** cung cấp dữ liệu và xử lý nghiệp vụ cho Frontend.
- Thiết kế database, viết migrations, tích hợp **Trefle API** và **VNPay Sandbox**.
- Triển khai các mẫu thiết kế: **Adapter** (thanh toán), **Decorator** (tính giá), **Facade** (xử lý đơn hàng).
- Đảm bảo bảo mật, phân quyền với **ASP.NET Core Identity + JWT**.
- Phối hợp với Frontend Agent để thống nhất API contract.

## 2. Công nghệ sử dụng
- **Framework:** .NET 8 (ASP.NET Core Web API)
- **ORM:** Entity Framework Core 8 (Code First)
- **Database:** SQL Server (local development) → MonsterASP.NET (free plan) cho production
- **Authentication:** ASP.NET Core Identity + JWT (JSON Web Tokens)
- **Tích hợp HTTP Client:** `IHttpClientFactory` (gọi Trefle API, VNPay)
- **Mapping:** AutoMapper (khuyến nghị) hoặc thủ công
- **Logging:** ILogger built-in hoặc Serilog
- **Testing:** xUnit (cho unit test logic nghiệp vụ)
- **Hosting:** Render (free plan) – dùng Web Service cho API

## 3. Kiến trúc tổng thể

### 3.1. Cấu trúc thư mục đề xuất
```
DigiXanh.API/
├── Controllers/               # API endpoints
├── Services/                   # Business logic
│   ├── Interfaces/
│   └── Implementations/
├── Patterns/                   # Design patterns implementations
│   ├── Adapter/
│   ├── Decorator/
│   └── Facade/
├── Data/                        # DbContext, Migrations
├── Models/                       # Entities (EF Core)
├── DTOs/                         # Request/Response models
├── Helpers/                       # Extensions, Constants, Middlewares
├── Configurations/                # Cấu hình (JWT, CORS, Services)
├── Migrations/
├── appsettings.json
└── Program.cs
```

### 3.2. Layers
- **Controllers:** Nhận request, gọi Service, trả về response. Không chứa logic nghiệp vụ.
- **Services:** Chứa logic xử lý chính (PlantService, OrderService, PaymentService, TrefleService).
- **Repositories:** (Tùy chọn) Có thể dùng trực tiếp DbContext qua service, nhưng nếu phức tạp thì nên tách repository.
- **Patterns:** Đặt trong thư mục riêng, được inject qua DI.

## 4. Cài đặt và cấu hình ban đầu

### 4.1. Tạo project
```bash
dotnet new webapi -n DigiXanh.API
cd DigiXanh.API
```

### 4.2. Thêm các NuGet packages
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add package Newtonsoft.Json  # (nếu cần xử lý JSON đặc biệt)
```

### 4.3. Cấu hình DbContext và Identity
Tạo `ApplicationDbContext` kế thừa `IdentityDbContext<ApplicationUser>`.

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    
    public DbSet<Plant> Plants { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Cart> Carts { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Cấu hình thêm (nếu cần)
    }
}
```

### 4.4. Cấu hình JWT trong `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DigiXanh;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "your-super-secret-key-with-at-least-32-characters",
    "Issuer": "DigiXanh",
    "Audience": "DigiXanhClient",
    "ExpireMinutes": 60
  },
  "Trefle": {
    "ApiKey": "your-trefle-api-key"
  },
  "VNPay": {
    "TmnCode": "your-tmn-code",
    "HashSecret": "your-hash-secret",
    "Url": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "https://your-fe-domain.com/payment-return",
    "IpnUrl": "https://your-api.onrender.com/api/payment/vnpay-ipn"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:4200", "https://yourusername.github.io"]
  },
  "Logging": { ... }
}
```

### 4.5. Cấu hình Services trong `Program.cs`
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Register services
builder.Services.AddScoped<IPlantService, PlantService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IPaymentAdapter, CashPaymentAdapter>();
builder.Services.AddScoped<IPaymentAdapter, VNPayPaymentAdapter>(); // Cần phân biệt qua keyed services hoặc factory
builder.Services.AddScoped<IPriceCalculator, BasePriceCalculator>();
builder.Services.AddScoped<OrderProcessingFacade>();
builder.Services.AddHttpClient<ITrefleService, TrefleService>();
builder.Services.AddAutoMapper(typeof(Program));
```

### 4.6. Tạo Migration đầu tiên
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4.7. Seed data (tài khoản admin mặc định)
Tạo class `DbInitializer` để tạo role Admin và user admin nếu chưa có.

## 5. Database Schema chi tiết

### 5.1. Entities

**ApplicationUser** (kế thừa IdentityUser)
- Thêm các field: `FullName`, `Address`, `PhoneNumber` (nếu cần)

**Category**
```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Plant> Plants { get; set; }
}
```

**Plant**
```csharp
public class Plant
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ScientificName { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    public string ImageUrl { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

**Order**
```csharp
public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } // Pending, Paid, Shipped, Delivered, Cancelled
    public string ShippingAddress { get; set; }
    public string Phone { get; set; }
    public PaymentMethod PaymentMethod { get; set; } // Cash, VNPay
    public string TransactionId { get; set; } // Từ VNPay
    public ICollection<OrderItem> OrderItems { get; set; }
}

public enum OrderStatus
{
    Pending,
    Paid,
    Shipped,
    Delivered,
    Cancelled
}

public enum PaymentMethod
{
    Cash,
    VNPay
}
```

**OrderItem**
```csharp
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; }
    public int PlantId { get; set; }
    public Plant Plant { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Giá tại thời điểm mua
}
```

**Cart**
```csharp
public class Cart
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public int PlantId { get; set; }
    public Plant Plant { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

## 6. Chi tiết Design Patterns

### 6.1. Adapter Pattern – Thanh toán
**Interface:**
```csharp
public interface IPaymentAdapter
{
    Task<PaymentResult> ProcessPaymentAsync(Order order, PaymentInfo paymentInfo);
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string TransactionId { get; set; }
    public string PaymentUrl { get; set; } // Dùng cho VNPay
    public string Message { get; set; }
}

public class PaymentInfo
{
    public string ReturnUrl { get; set; }
    public string IpAddress { get; set; }
    // Các thông tin khác
}
```

**CashPaymentAdapter**
```csharp
public class CashPaymentAdapter : IPaymentAdapter
{
    public async Task<PaymentResult> ProcessPaymentAsync(Order order, PaymentInfo paymentInfo)
    {
        // Logic đơn giản: chuyển trạng thái order thành Pending (chờ xác nhận)
        order.Status = OrderStatus.Pending;
        order.PaymentMethod = PaymentMethod.Cash;
        return new PaymentResult { Success = true, Message = "Đơn hàng đã được đặt, chờ xác nhận" };
    }
}
```

**VNPayPaymentAdapter**
```csharp
public class VNPayPaymentAdapter : IPaymentAdapter
{
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public VNPayPaymentAdapter(IConfiguration config, IHttpContextAccessor httpContextAccessor)
    {
        _config = config;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(Order order, PaymentInfo paymentInfo)
    {
        // Xây dựng URL thanh toán VNPay
        string vnp_ReturnUrl = _config["VNPay:ReturnUrl"];
        string vnp_Url = _config["VNPay:Url"];
        string vnp_TmnCode = _config["VNPay:TmnCode"];
        string vnp_HashSecret = _config["VNPay:HashSecret"];
        
        var vnpay = new VnPayLibrary();
        vnpay.AddRequestData("vnp_Version", "2.1.0");
        vnpay.AddRequestData("vnp_Command", "pay");
        vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
        vnpay.AddRequestData("vnp_Amount", (order.TotalAmount * 100).ToString()); // VNPay yêu cầu *100
        vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", "VND");
        vnpay.AddRequestData("vnp_IpAddr", paymentInfo.IpAddress);
        vnpay.AddRequestData("vnp_Locale", "vn");
        vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang {order.Id}");
        vnpay.AddRequestData("vnp_OrderType", "other");
        vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
        vnpay.AddRequestData("vnp_TxnRef", order.Id.ToString());

        string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
        
        return new PaymentResult 
        { 
            Success = true, 
            PaymentUrl = paymentUrl,
            Message = "Chuyển hướng đến cổng thanh toán VNPay"
        };
    }
}
```

**Sử dụng:** Trong `OrderProcessingFacade`, cần có cách chọn đúng adapter dựa vào PaymentMethod. Có thể dùng **Factory pattern** hoặc **keyed services** (.NET 8 hỗ trợ).

Ví dụ dùng Factory:
```csharp
public interface IPaymentAdapterFactory
{
    IPaymentAdapter GetAdapter(PaymentMethod method);
}

public class PaymentAdapterFactory : IPaymentAdapterFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentAdapterFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPaymentAdapter GetAdapter(PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.Cash => _serviceProvider.GetRequiredService<CashPaymentAdapter>(),
            PaymentMethod.VNPay => _serviceProvider.GetRequiredService<VNPayPaymentAdapter>(),
            _ => throw new NotSupportedException()
        };
    }
}
```

### 6.2. Decorator Pattern – Tính giá giảm dần
**Interface:**
```csharp
public interface IPriceCalculator
{
    decimal CalculatePrice(IEnumerable<CartItem> items);
}
```

**BasePriceCalculator**
```csharp
public class BasePriceCalculator : IPriceCalculator
{
    public decimal CalculatePrice(IEnumerable<CartItem> items)
    {
        return items.Sum(i => i.Quantity * i.UnitPrice);
    }
}
```

**QuantityDiscountDecorator**
```csharp
public class QuantityDiscountDecorator : IPriceCalculator
{
    private readonly IPriceCalculator _inner;
    private readonly int _threshold;
    private readonly decimal _discountPercent;

    public QuantityDiscountDecorator(IPriceCalculator inner, int threshold, decimal discountPercent)
    {
        _inner = inner;
        _threshold = threshold;
        _discountPercent = discountPercent;
    }

    public decimal CalculatePrice(IEnumerable<CartItem> items)
    {
        var baseTotal = _inner.CalculatePrice(items);
        var totalQuantity = items.Sum(i => i.Quantity);
        
        if (totalQuantity >= _threshold)
        {
            // Áp dụng mức giảm cao nhất (nếu có nhiều decorator, cần logic chọn max)
            // Ở đây giả sử chain decorator theo thứ tự, decorator sau có thể override
            return baseTotal * (1 - _discountPercent / 100);
        }
        return baseTotal;
    }
}
```

**Cách sử dụng trong OrderProcessingFacade:**
```csharp
var calculator = new BasePriceCalculator();
calculator = new QuantityDiscountDecorator(calculator, 2, 5);
calculator = new QuantityDiscountDecorator(calculator, 3, 7);
var total = calculator.CalculatePrice(cart.Items);
```
**Lưu ý:** Cần đảm bảo logic giảm giá đúng: nếu tổng số lượng >=3 thì áp dụng 7%, không cộng dồn với 5%. Decorator thứ hai nên kiểm tra nếu totalQuantity >=3 thì ghi đè kết quả của decorator trước.

### 6.3. Facade Pattern – Xử lý đơn hàng
**OrderProcessingFacade** (chi tiết như trong context.md)

```csharp
public class OrderProcessingFacade
{
    private readonly IPriceCalculator _priceCalculator;
    private readonly IPaymentAdapterFactory _paymentFactory;
    private readonly IOrderRepository _orderRepo;
    private readonly IEmailService _emailService;
    private readonly ICartService _cartService;
    private readonly ApplicationDbContext _dbContext;

    public OrderProcessingFacade(
        IPriceCalculator priceCalculator,
        IPaymentAdapterFactory paymentFactory,
        IOrderRepository orderRepo,
        IEmailService emailService,
        ICartService cartService,
        ApplicationDbContext dbContext)
    {
        _priceCalculator = priceCalculator;
        _paymentFactory = paymentFactory;
        _orderRepo = orderRepo;
        _emailService = emailService;
        _cartService = cartService;
        _dbContext = dbContext;
    }

    public async Task<OrderResult> PlaceOrderAsync(string userId, CartDto cart, ShippingInfo info, PaymentMethod method)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // 1. Validate (kiểm tra giỏ hàng có item không, thông tin giao hàng)
            if (cart == null || !cart.Items.Any())
                throw new Exception("Giỏ hàng trống");

            // 2. Tính tổng tiền
            var total = _priceCalculator.CalculatePrice(cart.Items);

            // 3. Tạo Order entity
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = total,
                Status = OrderStatus.Pending,
                ShippingAddress = info.Address,
                Phone = info.Phone,
                PaymentMethod = method,
                OrderItems = cart.Items.Select(i => new OrderItem
                {
                    PlantId = i.PlantId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            // 4. Lưu order trước để có Id cho thanh toán (VNPay cần order Id)
            await _orderRepo.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            // 5. Xử lý thanh toán qua adapter
            var paymentAdapter = _paymentFactory.GetAdapter(method);
            var paymentInfo = new PaymentInfo 
            { 
                IpAddress = GetIpAddress(),
                ReturnUrl = info.ReturnUrl 
            };
            var paymentResult = await paymentAdapter.ProcessPaymentAsync(order, paymentInfo);
            
            if (!paymentResult.Success)
                throw new Exception(paymentResult.Message);

            // 6. Cập nhật transactionId nếu có
            if (!string.IsNullOrEmpty(paymentResult.TransactionId))
            {
                order.TransactionId = paymentResult.TransactionId;
                await _dbContext.SaveChangesAsync();
            }

            // 7. Gửi email xác nhận (nếu có dịch vụ)
            await _emailService.SendOrderConfirmationAsync(userId, order);

            // 8. Xoá giỏ hàng
            await _cartService.ClearCartAsync(userId);

            // 9. Commit transaction
            await transaction.CommitAsync();

            return new OrderResult 
            { 
                Success = true, 
                OrderId = order.Id,
                PaymentUrl = paymentResult.PaymentUrl // Để FE redirect nếu cần
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

## 7. Tích hợp Trefle API

### 7.1. Tạo TrefleService
```csharp
public interface ITrefleService
{
    Task<TrefleSearchResult> SearchPlantsAsync(string query);
    Task<TreflePlantDetail> GetPlantDetailAsync(int id);
}

public class TrefleService : ITrefleService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public TrefleService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
        _httpClient.BaseAddress = new Uri("https://trefle.io/api/v1/");
    }

    public async Task<TrefleSearchResult> SearchPlantsAsync(string query)
    {
        var response = await _httpClient.GetAsync($"plants/search?q={query}&token={_config["Trefle:ApiKey"]}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TrefleSearchResult>();
    }

    public async Task<TreflePlantDetail> GetPlantDetailAsync(int id)
    {
        var response = await _httpClient.GetAsync($"plants/{id}?token={_config["Trefle:ApiKey"]}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TreflePlantDetail>();
    }
}
```

### 7.2. API endpoints cho Admin
- `GET /api/admin/trefle/search?q={query}` → gọi TrefleService, trả về danh sách cây.
- `GET /api/admin/trefle/{id}` → lấy chi tiết cây từ Trefle.
- `POST /api/admin/plants` → nhận dữ liệu từ form (có thể kèm ảnh), lưu vào DB.

**Lưu ý:** Dữ liệu từ Trefle chỉ để tham khảo, admin vẫn phải nhập giá và danh mục.

## 8. Xác thực và Phân quyền (JWT + Identity)

### 8.1. Tạo endpoint AuthController
- `POST /api/auth/register`: nhận email, password, fullname → tạo user với role "User".
- `POST /api/auth/login`: xác thực → trả về JWT token.
- `POST /api/auth/refresh`: refresh token (nếu có).
- `GET /api/auth/me`: lấy thông tin user hiện tại.

### 8.2. Sinh JWT Token
```csharp
var tokenHandler = new JwtSecurityTokenHandler();
var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role ?? "User")
    }),
    Expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpireMinutes"])),
    Issuer = _config["Jwt:Issuer"],
    Audience = _config["Jwt:Audience"],
    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
};
var token = tokenHandler.CreateToken(tokenDescriptor);
return tokenHandler.WriteToken(token);
```

### 8.3. Bảo vệ endpoints
- `[Authorize]` cho các endpoint yêu cầu đăng nhập (giỏ hàng, đặt hàng).
- `[Authorize(Roles = "Admin")]` cho các endpoint admin (quản lý cây, dashboard).

## 9. API Endpoints chính (cần thống nhất với FE)

### 9.1. Public (không cần token)
- `GET /api/plants` – danh sách cây (có phân trang, tìm kiếm)
- `GET /api/plants/{id}` – chi tiết cây

### 9.2. Auth
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/logout` (nếu cần)
- `GET /api/auth/profile`

### 9.3. Cart (cần token)
- `GET /api/cart` – lấy giỏ hàng hiện tại
- `POST /api/cart/items` – thêm item
- `PUT /api/cart/items/{plantId}` – cập nhật số lượng
- `DELETE /api/cart/items/{plantId}` – xoá item
- `DELETE /api/cart` – xoá toàn bộ

### 9.4. Order (cần token)
- `POST /api/orders` – tạo đơn hàng mới (gọi facade)
- `GET /api/orders` – lịch sử đơn hàng của user
- `GET /api/orders/{id}` – chi tiết đơn hàng

### 9.5. Admin (cần token + role Admin)
- `GET /api/admin/plants` – quản lý cây
- `POST /api/admin/plants` – thêm cây
- `PUT /api/admin/plants/{id}` – sửa
- `DELETE /api/admin/plants/{id}` – xoá (soft delete)
- `GET /api/admin/trefle/search`
- `GET /api/admin/trefle/{id}`
- `GET /api/admin/dashboard` – thống kê (tổng đơn, doanh thu...)

## 10. Xử lý lỗi và Validation

### 10.1. Global Exception Middleware
Tạo middleware bắt tất cả exception, trả về `ProblemDetails` chuẩn.
```csharp
app.UseMiddleware<ExceptionMiddleware>();
```

### 10.2. Validation
- Dùng **FluentValidation** hoặc **Data Annotations** trong DTO.
- Trả về `400 Bad Request` kèm chi tiết lỗi.

## 11. Kiểm thử (Testing)

### 11.1. Unit test với xUnit
- Viết test cho `PriceCalculator` (decorator).
- Viết test cho `OrderProcessingFacade` (mock các dependency).
- Viết test cho `TrefleService` (mock HttpClient).

### 11.2. Integration test (tùy chọn)
- Test các endpoint với `WebApplicationFactory`.

## 12. Triển khai lên Render

### 12.1. Chuẩn bị
- Tạo file `Dockerfile` hoặc dùng Render Git deployment.
- Đảm bảo `appsettings.Production.json` hoặc biến môi trường cho các key nhạy cảm.

### 12.2. Các biến môi trường cần set trên Render
- `ConnectionStrings__Default` (chuỗi kết nối MonsterASP.NET)
- `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`
- `Trefle__ApiKey`
- `VNPay__TmnCode`, `VNPay__HashSecret`, `VNPay__Url`, `VNPay__ReturnUrl`, `VNPay__IpnUrl`
- `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1` (FE URLs)

### 12.3. Lưu ý
- Render free plan sẽ sleep sau 15 phút không hoạt động, lần request đầu tiên sẽ chậm.
- Dùng `dotnet ef database update` trong quá trình deploy (có thể chạy script riêng).

## 13. Phối hợp với Frontend Agent

### 13.1. Thống nhất API contract
- Cung cấp file Swagger/OpenAPI cho FE tham khảo.
- Trao đổi qua chat hoặc comment trên task.

### 13.2. Mock data (nếu FE cần test trước)
- Có thể tạo endpoint trả về dữ liệu mẫu.

### 13.3. Xử lý CORS
- Đảm bảo CORS cho phép đúng origin (GitHub Pages và localhost).

## 14. Quy trình làm việc cụ thể khi nhận task

1. **Đọc task** từ PO (ví dụ: US05 - Thêm cây mới từ Trefle API).
2. **Đọc lại context.md** và **backend-instruction.md**.
3. **Trao đổi với FE agent** nếu cần thống nhất API (endpoint, request/response).
4. **Tạo nhánh** từ `develop`: `feature/US05-trefle-integration`.
5. **Phát triển**:
   - Tạo/ sửa DTOs, Entities, Services.
   - Viết unit test cho logic mới.
   - Cập nhật migrations nếu thay đổi DB.
6. **Tự kiểm thử** với Postman/Swagger.
7. **Commit** với message chuẩn, push lên remote.
8. **Tạo Pull Request** vào `develop`, gán PO review.
9. **Phản hồi feedback**, sửa nếu cần.
10. **Sau khi merge**, xoá nhánh, cập nhật board.

---

**Chúc bạn làm việc hiệu quả và có những trải nghiệm học hỏi thú vị! Nếu có thắc mắc, hãy hỏi PO ngay nhé.** 🌿