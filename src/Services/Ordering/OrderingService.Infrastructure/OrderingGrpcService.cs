using Grpc.Core;
using MediatR;
using OrderingService.Application;
using OrderingService.Contracts;

namespace OrderingService.Infrastructure;

public sealed class OrderingGrpcService(IMediator mediator) : Contracts.OrderingGrpc.OrderingGrpcBase
{
    public override async Task<GetOrderReply> GetOrder(GetOrderRequest request, ServerCallContext context)
    {
        var order = await mediator.Send(new GetOrderByIdQuery(Guid.Parse(request.OrderId)), context.CancellationToken);
        if (order is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "找不到訂單。"));
        }

        // 這裡做的事情很單純：
        // 把 Application 回傳的 DTO 轉回 gRPC reply。
        var reply = new GetOrderReply
        {
            OrderId = order.OrderId.ToString(),
            CustomerEmail = order.CustomerEmail,
            Status = order.Status,
            TotalAmount = (double)order.TotalAmount
        };

        reply.Items.AddRange(order.Items.Select(x => new OrderItemReply
        {
            ProductId = x.ProductId.ToString(),
            Sku = x.Sku,
            ProductName = x.ProductName,
            UnitPrice = (double)x.UnitPrice,
            Quantity = x.Quantity
        }));

        return reply;
    }
}
