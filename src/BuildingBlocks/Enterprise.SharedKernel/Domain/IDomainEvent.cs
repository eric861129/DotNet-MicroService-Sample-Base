namespace Enterprise.SharedKernel.Domain;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
