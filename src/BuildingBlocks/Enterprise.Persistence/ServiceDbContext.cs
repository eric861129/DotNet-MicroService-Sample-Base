using Enterprise.Application.Abstractions;
using Enterprise.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.Persistence;

public abstract class ServiceDbContext(DbContextOptions options)
    : DbContext(options), IUnitOfWork, IOutboxStore, IInboxStore
{
    // OutboxMessages 代表尚未發送到 Event Bus 的事件。
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    // InboxMessages 用來記錄 consumer 已經處理過哪些事件，避免重複執行。
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public Task AddAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        MessagingTelemetry.RecordOutboxEnqueued(GetType().Name, integrationEvent.GetType().Name);
        OutboxMessages.Add(OutboxSerializer.Serialize(integrationEvent));
        return Task.CompletedTask;
    }

    public Task<bool> HasProcessedAsync(Guid eventId, string consumer, CancellationToken cancellationToken = default)
        => InboxMessages.AnyAsync(x => x.EventId == eventId && x.Consumer == consumer, cancellationToken);

    public async Task MarkProcessedAsync(Guid eventId, string consumer, CancellationToken cancellationToken = default)
    {
        InboxMessages.Add(new InboxMessage
        {
            EventId = eventId,
            Consumer = consumer
        });

        await SaveChangesAsync(cancellationToken);
    }

    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("OutboxMessages");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.ProcessedOnUtc, x.OccurredOnUtc });
            builder.Property(x => x.EventType).HasMaxLength(512);
            builder.Property(x => x.Payload).HasColumnType("nvarchar(max)");
            builder.Property(x => x.Version).HasMaxLength(32);
        });

        modelBuilder.Entity<InboxMessage>(builder =>
        {
            builder.ToTable("InboxMessages");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.EventId, x.Consumer }).IsUnique();
            builder.Property(x => x.Consumer).HasMaxLength(256);
        });

        ConfigureDomain(modelBuilder);
    }

    protected abstract void ConfigureDomain(ModelBuilder modelBuilder);
}
