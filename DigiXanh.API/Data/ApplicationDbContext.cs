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
    }
}