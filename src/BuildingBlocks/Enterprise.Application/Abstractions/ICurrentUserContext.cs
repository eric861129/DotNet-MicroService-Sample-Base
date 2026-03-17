namespace Enterprise.Application.Abstractions;

public interface ICurrentUserContext
{
    string SubjectId { get; }

    string? DisplayName { get; }

    IReadOnlyCollection<string> Roles { get; }

    bool IsAuthenticated { get; }
}
