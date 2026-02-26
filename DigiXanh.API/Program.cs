using System.Text;
using DigiXanh.API.Constants;
using DigiXanh.API.Data;
using DigiXanh.API.Models;
using DigiXanh.API.Services.Implementations;
using DigiXanh.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpClient<ITrefleService, TrefleService>(client =>
{
    client.BaseAddress = new Uri("https://trefle.io/api/v1/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

await EnsureDatabaseReadyAsync(app.Services);
await SeedIdentityDataAsync(app.Services);
await SeedCategoriesAsync(app.Services);
if (app.Environment.IsDevelopment())
{
    await SeedOrdersAsync(app.Services);
}

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
            Status = OrderStatus.Paid,
            OrderDate = today.AddHours(9)
        },
        new Order
        {
            TotalAmount = 320_000m,
            Status = OrderStatus.Delivered,
            OrderDate = today.AddHours(14)
        },
        new Order
        {
            TotalAmount = 280_000m,
            Status = OrderStatus.Pending,
            OrderDate = today.AddHours(16)
        },
        new Order
        {
            TotalAmount = 410_000m,
            Status = OrderStatus.Delivered,
            OrderDate = today.AddDays(-1).AddHours(10)
        },
        new Order
        {
            TotalAmount = 250_000m,
            Status = OrderStatus.Paid,
            OrderDate = today.AddDays(-2).AddHours(11)
        },
        new Order
        {
            TotalAmount = 190_000m,
            Status = OrderStatus.Cancelled,
            OrderDate = today.AddDays(-3).AddHours(9)
        },
        new Order
        {
            TotalAmount = 670_000m,
            Status = OrderStatus.Delivered,
            OrderDate = today.AddDays(-4).AddHours(15)
        },
        new Order
        {
            TotalAmount = 230_000m,
            Status = OrderStatus.Pending,
            OrderDate = today.AddDays(-5).AddHours(13)
        },
        new Order
        {
            TotalAmount = 730_000m,
            Status = OrderStatus.Paid,
            OrderDate = today.AddDays(-6).AddHours(8)
        },
        new Order
        {
            TotalAmount = 360_000m,
            Status = OrderStatus.Shipped,
            OrderDate = today.AddDays(-8).AddHours(12)
        }
    };

    dbContext.Orders.AddRange(sampleOrders);
    await dbContext.SaveChangesAsync();
}

public partial class Program
{
}
