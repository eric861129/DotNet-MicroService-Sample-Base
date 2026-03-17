using Enterprise.Persistence;
using Microsoft.EntityFrameworkCore;
using OrderingService.Domain;

namespace OrderingService.Infrastructure;

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : ServiceDbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void ConfigureDomain(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(builder =>
        {
            // Order 是 Aggregate Root，所以一張訂單會帶著多個 OrderItem 一起存。
            builder.ToTable("Orders");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CustomerEmail).HasMaxLength(256);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            builder.Ignore(x => x.TotalAmount);
            builder.HasMany(typeof(OrderItem), "_items").WithOne().OnDelete(DeleteBehavior.Cascade);
            builder.Navigation("_items").UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<OrderItem>(builder =>
        {
            builder.ToTable("OrderItems");
            builder.HasKey(x => x.Id);
            builder.Property<Guid>("OrderId");
            builder.Property(x => x.Sku).HasMaxLength(64);
            builder.Property(x => x.ProductName).HasMaxLength(200);
            builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
            builder.Ignore(x => x.LineTotal);
        });
    }
}
