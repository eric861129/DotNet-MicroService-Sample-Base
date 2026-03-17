using InventoryService.Domain;

namespace InventoryService.Application;

public interface IInventoryRepository
{
    Task<InventoryItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task AddAsync(InventoryItem item, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<InventoryItem>> ListAsync(CancellationToken cancellationToken = default);
}
