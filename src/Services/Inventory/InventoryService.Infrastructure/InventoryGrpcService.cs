using Grpc.Core;
using InventoryService.Application;
using InventoryService.Contracts;
using MediatR;

namespace InventoryService.Infrastructure;

public sealed class InventoryGrpcService(IMediator mediator) : Contracts.InventoryGrpc.InventoryGrpcBase
{
    public override async Task<CheckAvailabilityReply> CheckAvailability(CheckAvailabilityRequest request, ServerCallContext context)
    {
        // gRPC request 先轉成 Query，再交給 Application 層處理。
        var result = await mediator.Send(
            new GetInventoryAvailabilityQuery(Guid.Parse(request.ProductId), request.RequiredQuantity),
            context.CancellationToken);

        return new CheckAvailabilityReply
        {
            ProductId = result.ProductId.ToString(),
            AvailableQuantity = result.AvailableQuantity,
            IsAvailable = result.IsAvailable
        };
    }
}
