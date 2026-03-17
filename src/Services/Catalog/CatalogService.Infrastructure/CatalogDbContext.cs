using CatalogService.Domain;
using Enterprise.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : ServiceDbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void ConfigureDomain(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(builder =>
        {
            // 這裡決定 Product 在資料庫中長什麼樣子，
            // 例如表名、索引、欄位長度與金額精度。
            builder.ToTable("Products");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Sku).IsUnique();
            builder.Property(x => x.Sku).HasMaxLength(64);
            builder.Property(x => x.Name).HasMaxLength(200);
            builder.Property(x => x.Price).HasPrecision(18, 2);
        });
    }
}
