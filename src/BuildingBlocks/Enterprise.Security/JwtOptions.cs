namespace Enterprise.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Security:Jwt";

    public string? Authority { get; init; }

    public string? Audience { get; init; }

    public bool RequireHttpsMetadata { get; init; } = false;
}
