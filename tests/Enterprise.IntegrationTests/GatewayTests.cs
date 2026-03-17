using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Gateway.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Enterprise.IntegrationTests;

public sealed class GatewayTests
{
    [Fact]
    public async Task Gateway_should_reject_anonymous_requests()
    {
        await using var catalogBackend = await TestBackendHost.StartAsync();
        await using var orderingBackend = await TestBackendHost.StartAsync();
        await using var factory = new GatewayTestFactory(catalogBackend.BaseAddress, orderingBackend.BaseAddress);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/catalog/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Gateway_should_proxy_requests_with_valid_token()
    {
        await using var catalogBackend = await TestBackendHost.StartAsync();
        await using var orderingBackend = await TestBackendHost.StartAsync();
        await using var factory = new GatewayTestFactory(catalogBackend.BaseAddress, orderingBackend.BaseAddress);
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateToken());
        using var response = await client.GetAsync("/catalog/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("/api/products");
    }

    private static string CreateToken()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("gateway-signing-key-1234567890123456"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "https://auth.local",
            Audience = "gateway-api",
            Expires = DateTime.UtcNow.AddMinutes(30),
            Subject = new ClaimsIdentity([new Claim("client_id", "gateway-client")]),
            SigningCredentials = credentials
        };

        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityTokenHandler().CreateToken(descriptor));
    }

    private sealed class GatewayTestFactory(string catalogAddress, string orderingAddress)
        : WebApplicationFactory<GatewayApiMarker>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Security:Jwt:Issuer"] = "https://auth.local",
                    ["Security:Jwt:Audience"] = "gateway-api",
                    ["Security:Jwt:SigningKey"] = "gateway-signing-key-1234567890123456",
                    ["ReverseProxy:Routes:catalog-route:ClusterId"] = "catalog-cluster",
                    ["ReverseProxy:Routes:catalog-route:Match:Path"] = "/catalog/{**catch-all}",
                    ["ReverseProxy:Routes:catalog-route:Transforms:0:PathPattern"] = "/api/{**catch-all}",
                    ["ReverseProxy:Routes:ordering-route:ClusterId"] = "ordering-cluster",
                    ["ReverseProxy:Routes:ordering-route:Match:Path"] = "/ordering/{**catch-all}",
                    ["ReverseProxy:Routes:ordering-route:Transforms:0:PathPattern"] = "/api/{**catch-all}",
                    ["ReverseProxy:Clusters:catalog-cluster:Destinations:catalog-destination:Address"] = catalogAddress,
                    ["ReverseProxy:Clusters:ordering-cluster:Destinations:ordering-destination:Address"] = orderingAddress
                });
            });
        }
    }

    private sealed class TestBackendHost(WebApplication app, string baseAddress) : IAsyncDisposable
    {
        public string BaseAddress { get; } = baseAddress;

        public static async Task<TestBackendHost> StartAsync()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            var app = builder.Build();
            app.MapGet("/api/{**catchAll}", (HttpContext context) => Results.Ok(new
            {
                path = context.Request.Path.ToString()
            }));

            await app.StartAsync();
            return new TestBackendHost(app, app.Urls.Single().TrimEnd('/') + "/");
        }

        public async ValueTask DisposeAsync()
        {
            await app.StopAsync();
            await app.DisposeAsync();
        }
    }
}
