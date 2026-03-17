using CatalogService.Application;
using CatalogService.Contracts;
using Grpc.Core;
using MediatR;

namespace CatalogService.Infrastructure;

public sealed class CatalogGrpcService(IMediator mediator) : CatalogGrpc.CatalogGrpcBase
{
    public override async Task<GetProductReply> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        // gRPC 入口不直接碰 Repository，
        // 它仍然透過 MediatR 走 Application，保持服務內部流程一致。
        var product = await mediator.Send(new GetProductQuery(Guid.Parse(request.ProductId)), context.CancellationToken);

        if (product is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "找不到商品。"));
        }

        return new GetProductReply
        {
            ProductId = product.ProductId.ToString(),
            Sku = product.Sku,
            Name = product.Name,
            Price = (double)product.Price
        };
    }
}
