using CatalogService.Contracts;
using Grpc.Core;
using OrderingService.Application;

namespace OrderingService.Infrastructure;

public sealed class CatalogGrpcClient(CatalogGrpc.CatalogGrpcClient client) : IProductCatalogClient
{
    public async Task<CatalogProductSnapshot?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            // 這個 adapter 的工作是把外部 gRPC 契約，
            // 轉成 Application 層更容易理解的 snapshot 物件。
            var reply = await client.GetProductAsync(new GetProductRequest
            {
                ProductId = productId.ToString()
            }, cancellationToken: cancellationToken);

            return new CatalogProductSnapshot(Guid.Parse(reply.ProductId), reply.Sku, reply.Name, (decimal)reply.Price);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.NotFound)
        {
            // gRPC 的 NotFound 轉成 null，讓上層用一般商業流程處理即可。
            return null;
        }
    }
}
