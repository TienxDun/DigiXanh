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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>()
            .Property(category => category.Name)
            .HasMaxLength(100);

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
            .HasOne(plant => plant.Category)
            .WithMany(category => category.Plants)
            .HasForeignKey(plant => plant.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Order configuration
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
            .HasOne(order => order.User)
            .WithMany()
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // OrderItem configuration
        builder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);

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
            .IsUnique();
    }
}
