using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Enterprise.ServiceDefaults;

public static class EnterpriseServiceDefaultsExtensions
{
    public static IServiceCollection AddEnterpriseServiceDefaults(this IServiceCollection services, IConfiguration configuration)
    {
        // 這裡放的是「幾乎每個服務都需要」的共同設定。
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddServiceDiscovery();
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
            {
                // 這是範本的寬鬆預設值，正式環境通常要改成白名單。
                policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(_ => true);
            });
        });

        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());

        // 共用 HttpClient 預設帶有 Resilience，方便做 retry / timeout / circuit breaker。
        services.AddHttpClient("default")
            .AddStandardResilienceHandler();

        return services;
    }

    public static WebApplication UseEnterpriseServiceDefaults(this WebApplication app)
    {
        // 中介流程的順序有意義：
        // 先接住例外，再做 CORS，接著才是認證與授權。
        app.UseExceptionHandler();
        app.UseCors("DefaultCors");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready");

        return app;
    }
}
