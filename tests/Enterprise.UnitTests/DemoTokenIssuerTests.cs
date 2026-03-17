using System.IdentityModel.Tokens.Jwt;
using AuthService.Api.Authentication;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Enterprise.UnitTests;

public sealed class DemoTokenIssuerTests
{
    [Fact]
    public void CreateAccessToken_should_embed_expected_claims()
    {
        var options = Options.Create(new JwtIssuerOptions
        {
            Issuer = "https://auth.local",
            Audience = "gateway-api",
            SigningKey = "base-lite-signing-key-123456789012345",
            AccessTokenLifetimeMinutes = 30
        });
        var issuedAt = new DateTimeOffset(2026, 03, 17, 09, 00, 00, TimeSpan.Zero);
        var issuer = new DemoTokenIssuer(options, new FixedTimeProvider(issuedAt));
        var client = new DemoClientOptions
        {
            ClientId = "base-lite-client",
            ClientSecret = "secret-value",
            AllowedScopes = ["catalog.read", "ordering.write"]
        };

        var token = issuer.CreateAccessToken(client, "catalog.read ordering.write");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Issuer.Should().Be("https://auth.local");
        jwt.Audiences.Should().ContainSingle("gateway-api");
        jwt.Claims.Should().Contain(x => x.Type == "client_id" && x.Value == "base-lite-client");
        jwt.Claims.Should().Contain(x => x.Type == "scope" && x.Value == "catalog.read ordering.write");
        jwt.ValidTo.Should().Be(issuedAt.AddMinutes(30).UtcDateTime);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
