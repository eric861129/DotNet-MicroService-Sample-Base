using Enterprise.Configuration;
using Enterprise.Observability;
using Enterprise.Security;
using Enterprise.ServiceDefaults;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 先接上集中設定與觀測性，這樣後續所有入口行為都能共享相同基底。
builder.AddEnterpriseConfiguration();
builder.AddEnterpriseObservability("Gateway");

builder.Services.AddEnterpriseServiceDefaults(builder.Configuration);
builder.Services.AddEnterpriseJwtAuthentication(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddRateLimiter(options =>
{
    // 這是示範用的固定視窗限流：
    // 1 分鐘內最多 60 次，超過就排隊，最多排 10 個。
    options.AddFixedWindowLimiter("gateway", limiter =>
    {
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.PermitLimit = 60;
        limiter.QueueLimit = 10;
    });
});

builder.Services
    .AddReverseProxy()
    // Gateway 路由全部從設定檔讀，未來要搬去 App Configuration 也比較容易。
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseEnterpriseServiceDefaults();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new { Service = "Gateway", Message = "Enterprise API Gateway" }));

// 真正的代理轉發入口在這裡，外部請求會先到 Gateway，再被送去對應微服務。
app.MapReverseProxy().RequireRateLimiting("gateway");

app.Run();

public partial class Program;
