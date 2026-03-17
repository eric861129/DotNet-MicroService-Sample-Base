using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;

namespace AuthService.Api;

public sealed class OpenIddictSeeder(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        await EnsureClientAsync(applicationManager, "gateway-client", "gateway-secret", ["gateway-api"], cancellationToken);
        await EnsureClientAsync(applicationManager, "catalog-service", "catalog-secret", ["catalog-api"], cancellationToken);
        await EnsureClientAsync(applicationManager, "ordering-service", "ordering-secret", ["ordering-api"], cancellationToken);
        await EnsureClientAsync(applicationManager, "inventory-service", "inventory-secret", ["inventory-api"], cancellationToken);
        await EnsureClientAsync(applicationManager, "notification-service", "notification-secret", ["notification-api"], cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task EnsureClientAsync(
        IOpenIddictApplicationManager applicationManager,
        string clientId,
        string clientSecret,
        IReadOnlyCollection<string> scopes,
        CancellationToken cancellationToken)
    {
        if (await applicationManager.FindByClientIdAsync(clientId, cancellationToken) is not null)
        {
            return;
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            DisplayName = clientId
        };

        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);

        foreach (var scope in scopes)
        {
            descriptor.Permissions.Add($"{OpenIddictConstants.Permissions.Prefixes.Scope}{scope}");
        }

        await applicationManager.CreateAsync(descriptor, cancellationToken);
    }
}
