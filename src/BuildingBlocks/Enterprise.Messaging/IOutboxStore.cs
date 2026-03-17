namespace Enterprise.Messaging;

public interface IOutboxStore
{
    Task AddAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
