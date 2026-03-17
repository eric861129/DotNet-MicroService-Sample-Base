namespace AuthService.Api.Authentication;

public sealed class DemoClientsOptions
{
    public const string SectionName = "Auth";

    public List<DemoClientOptions> Clients { get; init; } = [];
}
