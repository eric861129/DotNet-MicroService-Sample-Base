namespace OrderingService.Application;

public sealed record CatalogProductSnapshot(Guid ProductId, string Sku, string Name, decimal Price);

public sealed record InventoryAvailabilitySnapshot(Guid ProductId, int AvailableQuantity, bool IsAvailable);

public interface IProductCatalogClient
{
    Task<CatalogProductSnapshot?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default);
}

public interface IInventoryAvailabilityClient
{
    Task<InventoryAvailabilitySnapshot> CheckAvailabilityAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
}
