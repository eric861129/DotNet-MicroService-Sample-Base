using Microsoft.EntityFrameworkCore;

namespace CatalogService.Api.Data;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(builder =>
        {
            builder.ToTable("Products");
            builder.HasKey(product => product.Id);
            builder.Property(product => product.Sku).HasMaxLength(64).IsRequired();
            builder.Property(product => product.Name).HasMaxLength(200).IsRequired();
            builder.Property(product => product.Price).HasColumnType("decimal(18,2)");
        });
    }
}
