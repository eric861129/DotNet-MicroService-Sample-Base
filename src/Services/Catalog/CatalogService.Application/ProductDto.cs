namespace CatalogService.Application;

public sealed record ProductDto(Guid ProductId, string Sku, string Name, decimal Price);
