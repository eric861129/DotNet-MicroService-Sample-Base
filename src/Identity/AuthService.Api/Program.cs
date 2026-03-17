using AuthService.Api;
using Enterprise.Configuration;
using Enterprise.Observability;
using Enterprise.Persistence;
using Enterprise.ServiceDefaults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseConfiguration();
builder.AddEnterpriseObservability("AuthService");

builder.Services.AddEnterpriseServiceDefaults(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    // Auth Service 自己也要有資料庫，因為 OpenIddict 會把 client 與授權資料存下來。
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDb"));
    options.UseOpenIddict();
});

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<AuthDbContext>();
    })
    .AddServer(options =>
    {
        // 目前主打內部 service-to-service 的 client credentials flow。
        options.SetTokenEndpointUris("/connect/token");
        options.AllowClientCredentialsFlow();
        options.RegisterScopes("gateway-api", "catalog-api", "ordering-api", "inventory-api", "notification-api");

        // 這裡先用暫時金鑰方便開發，正式環境請改成 Key Vault / 憑證管理。
        options.AddEphemeralEncryptionKey()
            .AddEphemeralSigningKey();
        options.DisableAccessTokenEncryption();
        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough();
    });

builder.Services.AddHostedService<DatabaseMigrationHostedService<AuthDbContext>>();

// 啟動時先塞一組示範 client，讓本機可以直接測試 service token 流程。
builder.Services.AddHostedService<OpenIddictSeeder>();

var app = builder.Build();

app.UseEnterpriseServiceDefaults();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new { Service = "AuthService", TokenEndpoint = "/connect/token" }));
app.MapGet("/connect/health", () => Results.Ok(new { Status = "Healthy" }));

app.Run();

public partial class Program;
