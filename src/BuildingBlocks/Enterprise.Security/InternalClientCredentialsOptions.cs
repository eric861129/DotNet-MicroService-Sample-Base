namespace Enterprise.Security;

public sealed class InternalClientCredentialsOptions
{
    public const string SectionName = "Security:InternalAuth";

    public string TokenEndpoint { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public string Scope { get; init; } = string.Empty;
}
