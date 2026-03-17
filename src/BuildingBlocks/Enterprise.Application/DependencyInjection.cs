using System.Reflection;
using Enterprise.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddEnterpriseApplication(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        // 這裡把 Application 層常用的能力一次掛上去，
        // 這樣每個服務只要呼叫一行，就能拿到 MediatR、驗證器與 Pipeline。
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));
        services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);

        // Pipeline 的順序代表請求要經過哪些關卡：
        // 先驗證資料，再寫 log，最後才決定要不要存進資料庫。
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        return services;
    }
}
