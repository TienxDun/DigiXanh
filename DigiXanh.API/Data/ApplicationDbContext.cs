using DigiXanh.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DigiXanh.API.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ==================== ApplicationUser Configuration ====================
        builder.Entity<ApplicationUser>()
            .Property(u => u.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Entity<ApplicationUser>()
            .Property(u => u.Address)
            .HasMaxLength(500);

        builder.Entity<ApplicationUser>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.CreatedAt)
            .HasDatabaseName("IX_AspNetUsers_CreatedAt");

        // ==================== Category Configuration ====================
        builder.Entity<Category>()
            .Property(category => category.Name)
            .HasMaxLength(100);

        builder.Entity<Category>()
            .Property(c => c.DisplayOrder)
            .HasDefaultValue(0);

        builder.Entity<Category>()
            .Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.Entity<Category>()
            .Property(c => c.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Self-referencing for hierarchy
        builder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Category>()
            .HasIndex(c => new { c.IsActive, c.DisplayOrder })
            .HasDatabaseName("IX_Categories_IsActive_DisplayOrder");

        // ==================== Plant Configuration ====================
        builder.Entity<Plant>()
            .Property(plant => plant.Name)
            .HasMaxLength(200);

        builder.Entity<Plant>()
            .Property(plant => plant.ScientificName)
            .HasMaxLength(200);

        builder.Entity<Plant>()
            .Property(plant => plant.ImageUrl)
            .HasMaxLength(500);

        builder.Entity<Plant>()
            .Property(plant => plant.Price)
            .HasPrecision(18, 2);

        builder.Entity<Plant>()
            .Property(plant => plant.StockQuantity)
            .HasDefaultValue(null);

        builder.Entity<Plant>()
            .Property(plant => plant.IsActive)
            .HasDefaultValue(true);

        builder.Entity<Plant>()
            .Property(plant => plant.IsDeleted)
            .HasDefaultValue(false);

        builder.Entity<Plant>()
            .Property(plant => plant.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Entity<Plant>()
            .Property(plant => plant.UpdatedBy)
            .HasMaxLength(450);

        builder.Entity<Plant>()
            .HasOne(plant => plant.Category)
            .WithMany(category => category.Plants)
            .HasForeignKey(plant => plant.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Composite index for common query patterns
        builder.Entity<Plant>()
            .HasIndex(p => new { p.IsDeleted, p.IsActive, p.CreatedAt })
            .HasDatabaseName("IX_Plants_Filter_Sort");

        builder.Entity<Plant>()
            .HasIndex(p => p.Name)
            .HasDatabaseName("IX_Plants_Name");

        builder.Entity<Plant>()
            .HasIndex(p => p.ScientificName)
            .HasDatabaseName("IX_Plants_ScientificName");

        // Check constraints for data integrity
        builder.Entity<Plant>()
            .ToTable("Plants", b => b.HasCheckConstraint("CK_Plants_Price", "[Price] >= 0"));

        builder.Entity<Plant>()
            .ToTable("Plants", b => b.HasCheckConstraint("CK_Plants_StockQuantity", "[StockQuantity] IS NULL OR [StockQuantity] >= 0"));

        // ==================== CartItem Configuration ====================
        builder.Entity<CartItem>()
            .Property(ci => ci.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Entity<CartItem>()
            .Property(ci => ci.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Entity<CartItem>()
            .Property(ci => ci.ExpiresAt);

        builder.Entity<CartItem>()
            .HasOne(item => item.User)
            .WithMany()
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CartItem>()
            .HasOne(item => item.Plant)
            .WithMany()
            .HasForeignKey(item => item.PlantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CartItem>()
            .HasIndex(item => new { item.UserId, item.PlantId })
            .IsUnique()
            .HasDatabaseName("IX_CartItems_UserId_PlantId");

        builder.Entity<CartItem>()
            .HasIndex(item => item.ExpiresAt)
            .HasFilter("[ExpiresAt] IS NOT NULL")
            .HasDatabaseName("IX_CartItems_ExpiresAt");

        // Check constraint
        builder.Entity<CartItem>()
            .ToTable("CartItems", b => b.HasCheckConstraint("CK_CartItems_Quantity", "[Quantity] > 0"));

        // ==================== Order Configuration ====================
        builder.Entity<Order>()
            .Property(order => order.TotalAmount)
            .HasPrecision(18, 2);

        builder.Entity<Order>()
            .Property(order => order.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Entity<Order>()
            .Property(order => order.FinalAmount)
            .HasPrecision(18, 2);

        builder.Entity<Order>()
            .Property(order => order.RecipientName)
            .HasMaxLength(200);

        builder.Entity<Order>()
            .Property(order => order.Phone)
            .HasMaxLength(20);

        builder.Entity<Order>()
            .Property(order => order.ShippingAddress)
            .HasMaxLength(500);

        builder.Entity<Order>()
            .Property(order => order.TransactionId)
            .HasMaxLength(100);

        builder.Entity<Order>()
            .Property(order => order.PaymentUrl)
            .HasMaxLength(2000);

        builder.Entity<Order>()
            .Property(order => order.OrderDate)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Entity<Order>()
            .Property(order => order.UpdatedBy)
            .HasMaxLength(450);

        builder.Entity<Order>()
            .HasOne(order => order.User)
            .WithMany()
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for Order
        builder.Entity<Order>()
            .HasIndex(o => new { o.UserId, o.OrderDate })
            .HasDatabaseName("IX_Orders_UserId_OrderDate");

        builder.Entity<Order>()
            .HasIndex(o => o.Status)
            .HasDatabaseName("IX_Orders_Status");

        builder.Entity<Order>()
            .HasIndex(o => o.TransactionId)
            .HasFilter("[TransactionId] IS NOT NULL")
            .HasDatabaseName("IX_Orders_TransactionId");

        builder.Entity<Order>()
            .HasIndex(o => new { o.Status, o.OrderDate })
            .HasDatabaseName("IX_Orders_Status_OrderDate");

        // Check constraints
        builder.Entity<Order>()
            .ToTable("Orders", b => b.HasCheckConstraint("CK_Orders_TotalAmount", "[TotalAmount] >= 0"));

        builder.Entity<Order>()
            .ToTable("Orders", b => b.HasCheckConstraint("CK_Orders_DiscountAmount", "[DiscountAmount] >= 0"));

        builder.Entity<Order>()
            .ToTable("Orders", b => b.HasCheckConstraint("CK_Orders_FinalAmount", "[FinalAmount] >= 0"));

        // ==================== OrderItem Configuration ====================
        builder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);

        builder.Entity<OrderItem>()
            .Property(oi => oi.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Plant)
            .WithMany()
            .HasForeignKey(oi => oi.PlantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OrderItem>()
            .HasIndex(oi => oi.OrderId)
            .HasDatabaseName("IX_OrderItems_OrderId");

        builder.Entity<OrderItem>()
            .HasIndex(oi => oi.PlantId)
            .HasDatabaseName("IX_OrderItems_PlantId");

        // Check constraints
        builder.Entity<OrderItem>()
            .ToTable("OrderItems", b => b.HasCheckConstraint("CK_OrderItems_Quantity", "[Quantity] > 0"));

        builder.Entity<OrderItem>()
            .ToTable("OrderItems", b => b.HasCheckConstraint("CK_OrderItems_UnitPrice", "[UnitPrice] >= 0"));

        // ==================== OrderStatusHistory Configuration ====================
        builder.Entity<OrderStatusHistory>()
            .Property(osh => osh.ChangedBy)
            .HasMaxLength(450);

        builder.Entity<OrderStatusHistory>()
            .Property(osh => osh.Reason)
            .HasMaxLength(500);

        builder.Entity<OrderStatusHistory>()
            .Property(osh => osh.ChangedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Entity<OrderStatusHistory>()
            .HasOne(osh => osh.Order)
            .WithMany(o => o.StatusHistories)
            .HasForeignKey(osh => osh.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OrderStatusHistory>()
            .HasIndex(osh => new { osh.OrderId, osh.ChangedAt })
            .HasDatabaseName("IX_OrderStatusHistories_OrderId_ChangedAt");
    }
}
