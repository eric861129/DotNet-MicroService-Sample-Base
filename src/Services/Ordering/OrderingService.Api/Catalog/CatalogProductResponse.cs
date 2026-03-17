namespace OrderingService.Api.Catalog;

public sealed record CatalogProductResponse(Guid ProductId, string Sku, string Name, decimal Price);
