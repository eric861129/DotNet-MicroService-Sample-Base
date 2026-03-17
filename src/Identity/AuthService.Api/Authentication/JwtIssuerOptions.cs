namespace AuthService.Api.Authentication;

public sealed class JwtIssuerOptions
{
    public const string SectionName = "Auth:Jwt";

    public string Issuer { get; init; } = "https://auth.local";

    public string Audience { get; init; } = "gateway-api";

    public string SigningKey { get; init; } = "gateway-signing-key-1234567890123456";

    public int AccessTokenLifetimeMinutes { get; init; } = 60;
}
