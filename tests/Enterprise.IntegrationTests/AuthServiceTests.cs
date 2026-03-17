using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text.Json.Nodes;
using AuthService.Api;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Enterprise.IntegrationTests;

public sealed class AuthServiceTests : IClassFixture<WebApplicationFactory<AuthServiceMarker>>
{
    private readonly HttpClient _client;

    public AuthServiceTests(WebApplicationFactory<AuthServiceMarker> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Token_endpoint_should_issue_jwt_for_valid_client()
    {
        using var response = await _client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = "gateway-client",
            ["client_secret"] = "gateway-secret",
            ["scope"] = "catalog.read ordering.write"
        }));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();
        var token = payload["access_token"]!.GetValue<string>();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        payload["token_type"]!.GetValue<string>().Should().Be("Bearer");
        payload["scope"]!.GetValue<string>().Should().Be("catalog.read ordering.write");
        jwt.Claims.Should().Contain(x => x.Type == "client_id" && x.Value == "gateway-client");
        jwt.Claims.Should().Contain(x => x.Type == "scope" && x.Value == "catalog.read ordering.write");
    }

    [Fact]
    public async Task Token_endpoint_should_reject_invalid_client_credentials()
    {
        using var response = await _client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = "gateway-client",
            ["client_secret"] = "wrong-secret"
        }));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("invalid_client");
    }
}
