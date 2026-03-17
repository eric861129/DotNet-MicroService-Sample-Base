using Enterprise.Application.Abstractions;
using Enterprise.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("InventoryDb")));

        services.AddScoped<Application.IInventoryRepository, InventoryRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<InventoryDbContext>());
        services.AddScoped<IInboxStore>(provider => provider.GetRequiredService<InventoryDbContext>());

        return services;
    }
}
