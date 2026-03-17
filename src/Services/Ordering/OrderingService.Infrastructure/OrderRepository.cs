using Microsoft.EntityFrameworkCore;
using OrderingService.Application;
using OrderingService.Domain;

namespace OrderingService.Infrastructure;

public sealed class OrderRepository(OrderingDbContext dbContext) : IOrderRepository
{
    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        // 新增訂單時，EF Core 會連同 Aggregate 內的 OrderItem 一起追蹤。
        dbContext.Orders.Add(order);
        return Task.CompletedTask;
    }

    public Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        // 查訂單時連同項目一起載入，避免上層還要自己補第二次查詢。
        return dbContext.Orders
            .Include("_items")
            .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
    }
}
