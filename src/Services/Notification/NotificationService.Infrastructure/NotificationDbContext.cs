using Enterprise.Persistence;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain;

namespace NotificationService.Infrastructure;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : ServiceDbContext(options)
{
    public DbSet<NotificationLog> Notifications => Set<NotificationLog>();

    protected override void ConfigureDomain(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationLog>(builder =>
        {
            // 通知記錄主要就是保存「寄給誰、內容是什麼、什麼時候建立」。
            builder.ToTable("Notifications");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Recipient).HasMaxLength(256);
            builder.Property(x => x.Message).HasMaxLength(1024);
        });
    }
}
