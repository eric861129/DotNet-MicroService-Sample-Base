using System.Net;
using System.Net.Http.Json;
using CatalogService.Api;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Enterprise.IntegrationTests;

public sealed class CatalogServiceTests
{
    [Fact]
    public async Task Create_and_list_products_should_work()
    {
        await using var factory = new CatalogTestFactory();
        using var client = factory.CreateClient();

        using var createResponse = await client.PostAsJsonAsync("/api/products", new
        {
            sku = "SKU-001",
            name = "Base Lite Product",
            price = 1280m
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var products = await client.GetFromJsonAsync<List<ProductResponse>>("/api/products");
        products.Should().ContainSingle(x => x.Sku == "SKU-001" && x.Name == "Base Lite Product");
    }

    [Fact]
    public async Task Get_product_should_return_not_found_for_unknown_id()
    {
        await using var factory = new CatalogTestFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed class CatalogTestFactory : WebApplicationFactory<CatalogServiceMarker>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:CatalogDb"] = $"InMemory:catalog-tests-{Guid.NewGuid()}"
                });
            });
        }
    }
}
