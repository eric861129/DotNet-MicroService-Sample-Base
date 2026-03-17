using AuthService.Api.Authentication;
using Enterprise.ServiceDefaults;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEnterpriseServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.Configure<JwtIssuerOptions>(builder.Configuration.GetSection(JwtIssuerOptions.SectionName));
builder.Services.Configure<DemoClientsOptions>(builder.Configuration.GetSection(DemoClientsOptions.SectionName));
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<DemoTokenIssuer>();

var app = builder.Build();

app.UseEnterpriseServiceDefaults();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new { Service = "AuthService", Mode = "BaseLite", TokenEndpoint = "/connect/token" }));

app.MapPost("/connect/token", async (HttpRequest request, IOptions<DemoClientsOptions> clientOptions, DemoTokenIssuer tokenIssuer) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest(new { error = "invalid_request", error_description = "Form urlencoded payload is required." });
    }

    var form = await request.ReadFormAsync();
    if (!string.Equals(form["grant_type"], "client_credentials", StringComparison.Ordinal))
    {
        return Results.BadRequest(new { error = "unsupported_grant_type" });
    }

    var clientId = form["client_id"].ToString();
    var clientSecret = form["client_secret"].ToString();
    var requestedScope = form["scope"].ToString();

    var client = clientOptions.Value.Clients.FirstOrDefault(candidate =>
        string.Equals(candidate.ClientId, clientId, StringComparison.Ordinal) &&
        string.Equals(candidate.ClientSecret, clientSecret, StringComparison.Ordinal));

    if (client is null)
    {
        return Results.BadRequest(new { error = "invalid_client" });
    }

    if (!client.CanIssueScope(requestedScope))
    {
        return Results.BadRequest(new { error = "invalid_scope" });
    }

    var scope = string.IsNullOrWhiteSpace(requestedScope)
        ? string.Join(' ', client.AllowedScopes)
        : requestedScope.Trim();

    var token = tokenIssuer.CreateAccessToken(client, scope);

    return Results.Ok(new
    {
        access_token = token,
        token_type = "Bearer",
        expires_in = tokenIssuer.AccessTokenLifetime.TotalSeconds,
        scope
    });
});

app.Run();
