using System.Security.Claims;
using System.Text;
using DigiXanh.API.Constants;
using DigiXanh.API.Data;
using DigiXanh.API.Models;
using DigiXanh.API.Patterns.Adapter;
using DigiXanh.API.Patterns.Facade;
using DigiXanh.API.Services.Implementations;
using DigiXanh.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"]
             ?? throw new InvalidOperationException("Missing configuration: Jwt:Key");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpClient<ITrefleService, TrefleService>(client =>
{
    client.BaseAddress = new Uri("https://trefle.io/api/v1/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Register Payment Adapters (Adapter Pattern)
builder.Services.AddScoped<CashPaymentAdapter>();
builder.Services.AddScoped<VNPayPaymentAdapter>();
builder.Services.AddScoped<IPaymentAdapterFactory, PaymentAdapterFactory>();

// Register OrderProcessingFacade (Facade Pattern)
builder.Services.AddScoped<OrderProcessingFacade>();

// Register OrderEmailService (Placeholder for order confirmation emails)
builder.Services.AddScoped<IOrderEmailService, OrderEmailService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

await EnsureDatabaseReadyAsync(app.Services);
await SeedIdentityDataAsync(app.Services);
await SeedCategoriesAsync(app.Services);
// Skip seeding orders to avoid FK constraint issues with test data
// if (app.Environment.IsDevelopment())
// {
//     await SeedOrdersAsync(app.Services);
// }

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/health", () => "OK")
    .WithName("HealthCheck")
    .WithOpenApi();

app.Run();

static async Task EnsureDatabaseReadyAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (dbContext.Database.IsRelational())
    {
        await dbContext.Database.MigrateAsync();
        return;
    }

    await dbContext.Database.EnsureCreatedAsync();
}

static async Task SeedIdentityDataAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync(DefaultRoles.Admin))
    {
        await roleManager.CreateAsync(new IdentityRole(DefaultRoles.Admin));
    }

    if (!await roleManager.RoleExistsAsync(DefaultRoles.User))
    {
        await roleManager.CreateAsync(new IdentityRole(DefaultRoles.User));
    }

    var adminEmail = config["AdminSeed:Email"] ?? "admin@digixanh.com";
    var adminPassword = config["AdminSeed:Password"] ?? "Admin@123";
    var adminFullName = config["AdminSeed:FullName"] ?? "DigiXanh Admin";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser is null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = adminFullName,
            EmailConfirmed = true
        };

        var createAdminResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!createAdminResult.Succeeded)
        {
            var errors = string.Join("; ", createAdminResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Unable to seed default admin user: {errors}");
        }
    }

    if (!await userManager.IsInRoleAsync(adminUser, DefaultRoles.Admin))
    {
        var addRoleResult = await userManager.AddToRoleAsync(adminUser, DefaultRoles.Admin);
        if (!addRoleResult.Succeeded)
        {
            var errors = string.Join("; ", addRoleResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Unable to assign Admin role to default admin user: {errors}");
        }
    }
}

static async Task SeedCategoriesAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var defaultCategoryNames = new[]
    {
        "Cây để bàn",
        "Cây văn phòng",
        "Cây phong thủy"
    };

    var existingNames = await dbContext.Categories
        .AsNoTracking()
        .Select(category => category.Name)
        .ToListAsync();

    var toInsert = defaultCategoryNames
        .Where(name => !existingNames.Contains(name, StringComparer.OrdinalIgnoreCase))
        .Select(name => new Category { Name = name })
        .ToList();

    if (toInsert.Count == 0)
    {
        return;
    }

    dbContext.Categories.AddRange(toInsert);
    await dbContext.SaveChangesAsync();
}

static async Task SeedOrdersAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var hasOrders = await dbContext.Orders.AnyAsync();
    if (hasOrders)
    {
        return;
    }

    var today = DateTime.UtcNow.Date;
    var sampleOrders = new[]
    {
        new Order
        {
            TotalAmount = 550_000m,
            DiscountAmount = 0,
            FinalAmount = 550_000m,
            Status = OrderStatus.Paid,
            OrderDate = today.AddHours(9),
            RecipientName = "Nguyễn Văn A",
            Phone = "0901234567",
            ShippingAddress = "123 Lê Lợi, Q.1, TP.HCM",
            PaymentMethod = PaymentMethod.VNPay,
            UserId = "seed-user-1"
        },
        new Order
        {
            TotalAmount = 320_000m,
            DiscountAmount = 16_000m,
            FinalAmount = 304_000m,
            Status = OrderStatus.Delivered,
            OrderDate = today.AddHours(14),
            RecipientName = "Trần Thị B",
            Phone = "0912345678",
            ShippingAddress = "456 Nguyễn Huệ, Q.1, TP.HCM",
            PaymentMethod = PaymentMethod.Cash,
            UserId = "seed-user-2"
        },
        new Order
        {
            TotalAmount = 280_000m,
            DiscountAmount = 0,
            FinalAmount = 280_000m,
            Status = OrderStatus.Pending,
            OrderDate = today.AddHours(16),
            RecipientName = "Lê Văn C",
            Phone = "0923456789",
            ShippingAddress = "789 Đồng Khởi, Q.1, TP.HCM",
            PaymentMethod = PaymentMethod.Cash,
            UserId = "seed-user-3"
        },
        new Order
        {
            TotalAmount = 410_000m,
            DiscountAmount = 20_500m,
            FinalAmount = 389_500m,
            Status = OrderStatus.Delivered,
            OrderDate = today.AddDays(-1).AddHours(10),
            RecipientName = "Phạm Thị D",
            Phone = "0934567890",
            ShippingAddress = "321 Hai Bà Trưng, Q.3, TP.HCM",
            PaymentMethod = PaymentMethod.VNPay,
            UserId = "seed-user-4"
        },
        new Order
        {
            TotalAmount = 250_000m,
            DiscountAmount = 0,
            FinalAmount = 250_000m,
            Status = OrderStatus.Paid,
            OrderDate = today.AddDays(-2).AddHours(11),
            RecipientName = "Hoàng Văn E",
            Phone = "0945678901",
            ShippingAddress = "654 Võ Văn Tần, Q.3, TP.HCM",
            PaymentMethod = PaymentMethod.VNPay,
            UserId = "seed-user-5"
        },
        new Order
        {
            TotalAmount = 190_000m,
            DiscountAmount = 0,
            FinalAmount = 190_000m,
            Status = OrderStatus.Cancelled,
            OrderDate = today.AddDays(-3).AddHours(9),
            RecipientName = "Vũ Thị F",
            Phone = "0956789012",
            ShippingAddress = "987 Cách Mạng Tháng 8, Q.10, TP.HCM",
            PaymentMethod = PaymentMethod.Cash,
            UserId = "seed-user-6"
        },
        new Order
        {
            TotalAmount = 670_000m,
            DiscountAmount = 46_900m,
            FinalAmount = 623_100m,
            Status = OrderStatus.Delivered,
            OrderDate = today.AddDays(-4).AddHours(15),
            RecipientName = "Đỗ Văn G",
            Phone = "0967890123",
            ShippingAddress = "147 Nguyễn Trãi, Q.5, TP.HCM",
            PaymentMethod = PaymentMethod.VNPay,
            UserId = "seed-user-7"
        },
        new Order
        {
            TotalAmount = 230_000m,
            DiscountAmount = 0,
            FinalAmount = 230_000m,
            Status = OrderStatus.Pending,
            OrderDate = today.AddDays(-5).AddHours(13),
            RecipientName = "Bùi Thị H",
            Phone = "0978901234",
            ShippingAddress = "258 Lý Thường Kiệt, Q.10, TP.HCM",
            PaymentMethod = PaymentMethod.Cash,
            UserId = "seed-user-8"
        },
        new Order
        {
            TotalAmount = 730_000m,
            DiscountAmount = 51_100m,
            FinalAmount = 678_900m,
            Status = OrderStatus.Paid,
            OrderDate = today.AddDays(-6).AddHours(8),
            RecipientName = "Ngô Văn I",
            Phone = "0989012345",
            ShippingAddress = "369 Trần Hưng Đạo, Q.5, TP.HCM",
            PaymentMethod = PaymentMethod.VNPay,
            UserId = "seed-user-9"
        },
        new Order
        {
            TotalAmount = 360_000m,
            DiscountAmount = 18_000m,
            FinalAmount = 342_000m,
            Status = OrderStatus.Shipped,
            OrderDate = today.AddDays(-8).AddHours(12),
            RecipientName = "Dương Thị K",
            Phone = "0990123456",
            ShippingAddress = "753 Nguyễn Văn Cừ, Q.5, TP.HCM",
            PaymentMethod = PaymentMethod.Cash,
            UserId = "seed-user-10"
        }
    };

    dbContext.Orders.AddRange(sampleOrders);
    await dbContext.SaveChangesAsync();
}

public partial class Program
{
}
