using CatalogService.Application;
using CatalogService.Domain;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure;

public sealed class CatalogProductRepository(CatalogDbContext dbContext) : ICatalogProductRepository
{
    public Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        // Repository 幫 Application 隔開 EF Core 細節，
        // 讓上層只看到「新增商品」這件事。
        dbContext.Products.Add(product);
        return Task.CompletedTask;
    }

    public Task<Product?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return dbContext.Products.FirstOrDefaultAsync(x => x.Id == productId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Product>> ListAsync(CancellationToken cancellationToken = default)
    {
        // 讀清單時固定排序，讓 API 回傳結果比較穩定。
        return await dbContext.Products
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
