using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Messaging;

public static class MessagingExtensions
{
    public static IServiceCollection AddEnterpriseEventTypeRegistry(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddSingleton<IEventTypeRegistry>(_ => new EventTypeRegistry(assemblies));
        return services;
    }

    public static IServiceCollection AddEnterpriseMassTransit(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        var options = configuration.GetSection(MessagingTransportOptions.SectionName).Get<MessagingTransportOptions>()
            ?? new MessagingTransportOptions();

        services.AddMassTransit(registration =>
        {
            configureConsumers?.Invoke(registration);

            switch (options.Provider.ToLowerInvariant())
            {
                case "servicebus":
                case "azureservicebus":
                    registration.UsingAzureServiceBus((context, bus) =>
                    {
                        bus.Host(options.ServiceBus.ConnectionString);
                        bus.UseMessageRetry(retry => retry.Interval(3, TimeSpan.FromSeconds(2)));
                        bus.ConfigureEndpoints(context);
                    });
                    break;
                default:
                    registration.UsingRabbitMq((context, bus) =>
                    {
                        bus.Host(
                            options.RabbitMq.Host,
                            options.RabbitMq.Port,
                            options.RabbitMq.VirtualHost,
                            host =>
                            {
                                host.Username(options.RabbitMq.Username);
                                host.Password(options.RabbitMq.Password);
                            });

                        bus.UseMessageRetry(retry => retry.Interval(3, TimeSpan.FromSeconds(2)));
                        bus.ConfigureEndpoints(context);
                    });
                    break;
            }
        });

        return services;
    }
}
