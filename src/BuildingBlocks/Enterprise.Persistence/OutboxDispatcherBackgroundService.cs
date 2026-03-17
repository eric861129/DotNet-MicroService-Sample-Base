using Enterprise.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Enterprise.Persistence;

public sealed class OutboxDispatcherBackgroundService<TDbContext>(
    IServiceScopeFactory scopeFactory,
    IEventTypeRegistry eventTypeRegistry,
    ILogger<OutboxDispatcherBackgroundService<TDbContext>> logger)
    : BackgroundService
    where TDbContext : ServiceDbContext
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessBatchOnceAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    public Task ProcessBatchOnceAsync(CancellationToken cancellationToken = default)
        => ProcessBatchAsync(cancellationToken);

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var messages = await dbContext.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var integrationEvent = OutboxSerializer.Deserialize(message, eventTypeRegistry);
                await publishEndpoint.Publish(integrationEvent, integrationEvent.GetType(), cancellationToken);
                message.MarkProcessed();
                MessagingTelemetry.RecordOutboxPublished(typeof(TDbContext).Name, message.EventType);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to dispatch Outbox message: {EventType}", message.EventType);
                message.MarkFailed(exception.Message);
                MessagingTelemetry.RecordOutboxFailed(typeof(TDbContext).Name, message.EventType);
            }
            finally
            {
                stopwatch.Stop();
                MessagingTelemetry.RecordOutboxDispatchDuration(typeof(TDbContext).Name, message.EventType, stopwatch.Elapsed);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
