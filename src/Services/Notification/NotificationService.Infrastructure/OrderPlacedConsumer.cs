using Enterprise.Messaging;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Domain;
using OrderingService.Contracts;

namespace NotificationService.Infrastructure;

public sealed class OrderPlacedConsumer(
    NotificationDbContext dbContext,
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
            logger.LogInformation("Skip duplicated notification event: {EventId}", message.EventId);
            MessagingTelemetry.RecordConsumerDuplicate(ConsumerName, nameof(OrderPlacedIntegrationEvent));
            return;
        }

        try
        {
            var body = $"訂單 {message.OrderId} 已建立，總金額 {message.TotalAmount:C}.";
            dbContext.Notifications.Add(NotificationLog.Create(message.OrderId, message.CustomerEmail, body));

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
