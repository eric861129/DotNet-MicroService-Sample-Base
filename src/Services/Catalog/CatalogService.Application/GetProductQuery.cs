using Enterprise.Application.Abstractions;
using MediatR;

namespace CatalogService.Application;

public sealed record GetProductQuery(Guid ProductId) : IQuery<ProductDto?>;

public sealed record ListProductsQuery() : IQuery<IReadOnlyCollection<ProductDto>>;

public sealed class GetProductQueryHandler(ICatalogProductRepository repository)
    : IRequestHandler<GetProductQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.ProductId, cancellationToken);
        return product is null ? null : new ProductDto(product.Id, product.Sku, product.Name, product.Price);
    }
}

public sealed class ListProductsQueryHandler(ICatalogProductRepository repository)
    : IRequestHandler<ListProductsQuery, IReadOnlyCollection<ProductDto>>
{
    public async Task<IReadOnlyCollection<ProductDto>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await repository.ListAsync(cancellationToken);
        return products.Select(x => new ProductDto(x.Id, x.Sku, x.Name, x.Price)).ToArray();
    }
}
