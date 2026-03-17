using CatalogService.Domain;

namespace CatalogService.Application;

public interface ICatalogProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Product>> ListAsync(CancellationToken cancellationToken = default);
}
