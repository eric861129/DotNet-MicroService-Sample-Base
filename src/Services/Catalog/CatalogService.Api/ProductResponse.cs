namespace CatalogService.Api;

public sealed record ProductResponse(Guid ProductId, string Sku, string Name, decimal Price);
