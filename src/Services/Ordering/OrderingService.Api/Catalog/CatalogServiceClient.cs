using System.Net;
using System.Net.Http.Json;

namespace OrderingService.Api.Catalog;

public sealed class CatalogServiceClient(HttpClient httpClient) : ICatalogServiceClient
{
    public async Task<CatalogProductResponse?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"api/products/{productId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new CatalogServiceUnavailableException($"Catalog service returned {(int)response.StatusCode}.");
        }

        var product = await response.Content.ReadFromJsonAsync<CatalogProductResponse>(cancellationToken: cancellationToken);
        return product ?? throw new CatalogServiceUnavailableException("Catalog service returned an empty product payload.");
    }
}
