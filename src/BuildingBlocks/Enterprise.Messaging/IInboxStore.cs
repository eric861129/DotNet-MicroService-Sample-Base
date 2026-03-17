namespace Enterprise.Messaging;

public interface IInboxStore
{
    Task<bool> HasProcessedAsync(Guid eventId, string consumer, CancellationToken cancellationToken = default);

    Task MarkProcessedAsync(Guid eventId, string consumer, CancellationToken cancellationToken = default);
}
