namespace Enterprise.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Security:Jwt";

    public string Issuer { get; init; } = "https://auth.local";

    public string Audience { get; init; } = "gateway-api";

    public string SigningKey { get; init; } = "gateway-signing-key-1234567890123456";
}
