namespace OrderingService.Api.Catalog;

public interface ICatalogServiceClient
{
    Task<CatalogProductResponse?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default);
}
