using InventoryService.Contracts;
using OrderingService.Application;

namespace OrderingService.Infrastructure;

public sealed class InventoryGrpcClient(InventoryGrpc.InventoryGrpcClient client) : IInventoryAvailabilityClient
{
    public async Task<InventoryAvailabilitySnapshot> CheckAvailabilityAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        // Ordering 不需要知道 Inventory gRPC 的細節，
        // 它只想知道「這個商品剩多少、夠不夠」。
        var reply = await client.CheckAvailabilityAsync(new CheckAvailabilityRequest
        {
            ProductId = productId.ToString(),
            RequiredQuantity = quantity
        }, cancellationToken: cancellationToken);

        return new InventoryAvailabilitySnapshot(Guid.Parse(reply.ProductId), reply.AvailableQuantity, reply.IsAvailable);
    }
}
