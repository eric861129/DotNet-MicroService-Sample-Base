namespace Enterprise.SharedKernel.Domain;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;

    public DateTime? LastModifiedAtUtc { get; protected set; }

    public void Touch()
    {
        LastModifiedAtUtc = DateTime.UtcNow;
    }
}
