using EcomSearchApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EcomSearchApi.Data;

// Entity Framework Core database context for PostgreSQL
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    // Configures relational model mappings and custom database column conversions
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Tags)
                  .HasColumnType("text[]");

            entity.Property(p => p.CreatedAt)
                  .HasConversion(
                      v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                      v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                  );
        });
    }
}
