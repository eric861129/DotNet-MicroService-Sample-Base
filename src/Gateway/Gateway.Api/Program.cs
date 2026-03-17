using Enterprise.Security;
using Enterprise.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEnterpriseServiceDefaults();
builder.Services.AddEnterpriseJwtAuthentication(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseEnterpriseServiceDefaults();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new { Service = "Gateway", Mode = "BaseLite" }));
app.MapReverseProxy().RequireAuthorization();

app.Run();
