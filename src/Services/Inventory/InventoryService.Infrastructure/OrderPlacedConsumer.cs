using Enterprise.Messaging;
using InventoryService.Domain;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrderingService.Contracts;

namespace InventoryService.Infrastructure;

public sealed class OrderPlacedConsumer(
    InventoryDbContext dbContext,
    IInboxStore inboxStore,
    ILogger<OrderPlacedConsumer> logger)
    : IConsumer<OrderPlacedIntegrationEvent>
{
    private const string ConsumerName = nameof(OrderPlacedConsumer);

    public Task Consume(ConsumeContext<OrderPlacedIntegrationEvent> context)
        => ConsumeEventAsync(context.Message, context.CancellationToken, context.GetRetryAttempt());

    public async Task ConsumeEventAsync(
        OrderPlacedIntegrationEvent message,
        CancellationToken cancellationToken = default,
        int retryAttempt = 0)
    {
        MessagingTelemetry.RecordConsumerRetry(ConsumerName, nameof(OrderPlacedIntegrationEvent), retryAttempt);

        if (await inboxStore.HasProcessedAsync(message.EventId, ConsumerName, cancellationToken))
        {
            logger.LogInformation("Skip duplicated OrderPlaced event: {EventId}", message.EventId);
            MessagingTelemetry.RecordConsumerDuplicate(ConsumerName, nameof(OrderPlacedIntegrationEvent));
            return;
        }

        try
        {
            foreach (var item in message.Items)
            {
                var inventory = await dbContext.InventoryItems.FindAsync([item.ProductId], cancellationToken)
                    ?? throw new InvalidOperationException($"Inventory item {item.ProductId} was not found.");

                inventory.Reserve(item.Quantity);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await inboxStore.MarkProcessedAsync(message.EventId, ConsumerName, cancellationToken);
            MessagingTelemetry.RecordConsumerProcessed(ConsumerName, nameof(OrderPlacedIntegrationEvent));
        }
        catch
        {
            MessagingTelemetry.RecordConsumerFailure(ConsumerName, nameof(OrderPlacedIntegrationEvent));
            throw;
        }
    }
}
