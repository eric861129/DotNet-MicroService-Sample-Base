using InventoryService.Application;
using InventoryService.Domain;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure;

public sealed class InventoryRepository(InventoryDbContext dbContext) : IInventoryRepository
{
    public Task<InventoryItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return dbContext.InventoryItems.FirstOrDefaultAsync(x => x.Id == productId, cancellationToken);
    }

    public Task AddAsync(InventoryItem item, CancellationToken cancellationToken = default)
    {
        dbContext.InventoryItems.Add(item);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyCollection<InventoryItem>> ListAsync(CancellationToken cancellationToken = default)
    {
        // 依 SKU 排序後再回傳，方便人類閱讀與測試驗證。
        return await dbContext.InventoryItems
            .OrderBy(x => x.Sku)
            .ToListAsync(cancellationToken);
    }
}
