namespace Enterprise.Messaging;

public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required Guid EventId { get; init; }

    public required string EventType { get; init; }

    public required string Payload { get; init; }

    public required string Version { get; init; }

    public DateTime OccurredOnUtc { get; init; }

    public DateTime? ProcessedOnUtc { get; private set; }

    public string? Error { get; private set; }

    public void MarkProcessed()
    {
        ProcessedOnUtc = DateTime.UtcNow;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Error = error;
    }
}
