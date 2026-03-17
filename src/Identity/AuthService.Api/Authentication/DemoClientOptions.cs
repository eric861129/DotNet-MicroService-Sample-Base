namespace AuthService.Api.Authentication;

public sealed class DemoClientOptions
{
    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public List<string> AllowedScopes { get; init; } = [];

    public bool CanIssueScope(string requestedScope)
    {
        if (string.IsNullOrWhiteSpace(requestedScope))
        {
            return AllowedScopes.Count > 0;
        }

        var requested = requestedScope
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return requested.All(scope => AllowedScopes.Contains(scope, StringComparer.Ordinal));
    }
}
