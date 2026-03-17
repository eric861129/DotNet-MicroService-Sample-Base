using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Api.Authentication;

public sealed class DemoTokenIssuer(IOptions<JwtIssuerOptions> options, TimeProvider timeProvider)
{
    private readonly JwtIssuerOptions _options = options.Value;

    public TimeSpan AccessTokenLifetime => TimeSpan.FromMinutes(_options.AccessTokenLifetimeMinutes);

    public string CreateAccessToken(DemoClientOptions client, string scope)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(
            [
                new Claim("client_id", client.ClientId),
                new Claim("scope", scope)
            ]),
            Expires = now.Add(AccessTokenLifetime),
            NotBefore = now,
            IssuedAt = now,
            SigningCredentials = credentials
        };

        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityTokenHandler().CreateToken(tokenDescriptor));
    }
}
