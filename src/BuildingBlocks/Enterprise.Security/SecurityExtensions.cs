using Enterprise.Application.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Enterprise.Security;

public static class SecurityExtensions
{
    public static IServiceCollection AddEnterpriseJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = JwtOptions.SectionName)
    {
        // 這裡一次處理兩件事：
        // 1. 對外 JWT 驗證
        // 2. 對內 service-to-service token provider
        var jwtOptions = configuration.GetSection(sectionName).Get<JwtOptions>() ?? new JwtOptions();
        services.Configure<JwtOptions>(configuration.GetSection(sectionName));
        services.Configure<InternalClientCredentialsOptions>(configuration.GetSection(InternalClientCredentialsOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddHttpClient<IServiceTokenProvider, ClientCredentialsServiceTokenProvider>();

        // 沒有 Authority 時代表現在可能只是本機開發模式，
        // 這時先保留 auth 管線，但不強制要求真正的 JWT 驗證。
        if (string.IsNullOrWhiteSpace(jwtOptions.Authority))
        {
            services.AddAuthentication();
            services.AddAuthorization();
            return services;
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(bearerOptions =>
            {
                bearerOptions.Authority = string.IsNullOrWhiteSpace(jwtOptions.Authority) ? null : jwtOptions.Authority;
                bearerOptions.Audience = jwtOptions.Audience;
                bearerOptions.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = !string.IsNullOrWhiteSpace(jwtOptions.Audience)
                };
            });

        services.AddAuthorization();

        return services;
    }
}
