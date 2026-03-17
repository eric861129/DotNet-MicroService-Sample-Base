using Microsoft.EntityFrameworkCore;

namespace OrderingService.Api.Data;

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(builder =>
        {
            builder.ToTable("Orders");
            builder.HasKey(order => order.Id);
            builder.Property(order => order.CustomerEmail).HasMaxLength(320).IsRequired();
            builder.Property(order => order.TotalAmount).HasColumnType("decimal(18,2)");
            builder.Property(order => order.CreatedAtUtc).IsRequired();
            builder.Navigation(order => order.Items).AutoInclude();

            builder.OwnsMany(order => order.Items, item =>
            {
                item.ToTable("OrderItems");
                item.WithOwner().HasForeignKey("OrderId");
                item.Property<int>("Id");
                item.HasKey("Id");
                item.Property(orderItem => orderItem.ProductId).IsRequired();
                item.Property(orderItem => orderItem.Sku).HasMaxLength(64).IsRequired();
                item.Property(orderItem => orderItem.ProductName).HasMaxLength(200).IsRequired();
                item.Property(orderItem => orderItem.UnitPrice).HasColumnType("decimal(18,2)");
                item.Property(orderItem => orderItem.LineTotal).HasColumnType("decimal(18,2)");
            });
        });
    }
}
