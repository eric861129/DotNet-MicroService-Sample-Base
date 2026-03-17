using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Enterprise.ServiceDefaults;

public static class EnterpriseServiceDefaultsExtensions
{
    public static IServiceCollection AddEnterpriseServiceDefaults(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddAuthentication();
        services.AddAuthorization();
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
            {
                policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(_ => true);
            });
        });

        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());

        return services;
    }

    public static WebApplication UseEnterpriseServiceDefaults(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseCors("DefaultCors");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready");

        return app;
    }
}
