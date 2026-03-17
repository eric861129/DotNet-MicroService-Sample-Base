using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderingService.Api;
using OrderingService.Api.Catalog;

namespace Enterprise.IntegrationTests;

public sealed class OrderingServiceTests
{
    [Fact]
    public async Task Create_and_get_order_should_work_when_catalog_item_exists()
    {
        await using var factory = new OrderingTestFactory(new StubCatalogServiceClient(
            new CatalogProductResponse(Guid.Parse("11111111-1111-1111-1111-111111111111"), "SKU-001", "Starter Product", 1200m)));
        using var client = factory.CreateClient();

        using var createResponse = await client.PostAsJsonAsync("/api/orders", new
        {
            customerEmail = "student@example.com",
            items = new[]
            {
                new { productId = Guid.Parse("11111111-1111-1111-1111-111111111111"), quantity = 2 }
            }
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();

        var order = await client.GetFromJsonAsync<OrderResponse>($"/api/orders/{created!.OrderId}");
        order.Should().NotBeNull();
        order!.Items.Should().ContainSingle();
        order.TotalAmount.Should().Be(2400m);
    }

    [Fact]
    public async Task Create_order_should_fail_when_product_is_missing()
    {
        await using var factory = new OrderingTestFactory(new StubCatalogServiceClient(product: null));
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/orders", new
        {
            customerEmail = "student@example.com",
            items = new[]
            {
                new { productId = Guid.Parse("22222222-2222-2222-2222-222222222222"), quantity = 1 }
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("not found");
    }

    [Fact]
    public async Task Create_order_should_return_bad_gateway_when_catalog_is_unavailable()
    {
        await using var factory = new OrderingTestFactory(new ThrowingCatalogServiceClient());
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/orders", new
        {
            customerEmail = "student@example.com",
            items = new[]
            {
                new { productId = Guid.Parse("33333333-3333-3333-3333-333333333333"), quantity = 1 }
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Catalog service");
    }

    private sealed class OrderingTestFactory(ICatalogServiceClient catalogClient) : WebApplicationFactory<OrderingServiceMarker>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:OrderingDb"] = $"InMemory:ordering-tests-{Guid.NewGuid()}"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ICatalogServiceClient>();
                services.AddSingleton(catalogClient);
            });
        }
    }

    private sealed class StubCatalogServiceClient(CatalogProductResponse? product) : ICatalogServiceClient
    {
        public Task<CatalogProductResponse?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
            => Task.FromResult(product);
    }

    private sealed class ThrowingCatalogServiceClient : ICatalogServiceClient
    {
        public Task<CatalogProductResponse?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
            => throw new CatalogServiceUnavailableException("Catalog service is unavailable.");
    }
}
