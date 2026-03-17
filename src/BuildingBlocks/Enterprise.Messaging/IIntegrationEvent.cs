namespace Enterprise.Messaging;

public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTime OccurredOnUtc { get; }

    string Version { get; }
}
