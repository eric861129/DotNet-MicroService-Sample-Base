using CatalogService.Contracts;
using Enterprise.Application.Abstractions;
using Enterprise.Messaging;
using InventoryService.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OrderingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderingDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("OrderingDb")));

        services.AddScoped<Application.IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<OrderingDbContext>());
        services.AddScoped<IOutboxStore>(provider => provider.GetRequiredService<OrderingDbContext>());

        services.AddGrpcClient<CatalogGrpc.CatalogGrpcClient>(options =>
        {
            options.Address = new Uri(configuration["Services:Catalog:GrpcUrl"] ?? "http://localhost:7201");
        });

        services.AddGrpcClient<InventoryGrpc.InventoryGrpcClient>(options =>
        {
            options.Address = new Uri(configuration["Services:Inventory:GrpcUrl"] ?? "http://localhost:7202");
        });

        services.AddScoped<Application.IProductCatalogClient, CatalogGrpcClient>();
        services.AddScoped<Application.IInventoryAvailabilityClient, InventoryGrpcClient>();

        return services;
    }
}
