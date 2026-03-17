using Enterprise.Persistence;
using InventoryService.Domain;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : ServiceDbContext(options)
{
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    protected override void ConfigureDomain(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryItem>(builder =>
        {
            // Inventory 比較單純，主要關心商品 Id、SKU 與庫存數量。
            builder.ToTable("InventoryItems");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Sku).HasMaxLength(64);
        });
    }
}
