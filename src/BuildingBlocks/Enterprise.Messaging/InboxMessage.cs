namespace Enterprise.Messaging;

public sealed class InboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required Guid EventId { get; init; }

    public required string Consumer { get; init; }

    public DateTime ProcessedOnUtc { get; init; } = DateTime.UtcNow;
}
