using Enterprise.Application.Abstractions;
using MediatR;

namespace InventoryService.Application;

public sealed record GetInventoryAvailabilityQuery(Guid ProductId, int RequiredQuantity) : IQuery<InventoryAvailabilityDto>;

public sealed record ListInventoryItemsQuery() : IQuery<IReadOnlyCollection<InventoryItemDto>>;

public sealed record InventoryAvailabilityDto(Guid ProductId, int AvailableQuantity, bool IsAvailable);

public sealed class GetInventoryAvailabilityQueryHandler(IInventoryRepository repository)
    : IRequestHandler<GetInventoryAvailabilityQuery, InventoryAvailabilityDto>
{
    public async Task<InventoryAvailabilityDto> Handle(GetInventoryAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var item = await repository.GetByProductIdAsync(request.ProductId, cancellationToken);
        var availableQuantity = item?.AvailableQuantity ?? 0;

        return new InventoryAvailabilityDto(request.ProductId, availableQuantity, availableQuantity >= request.RequiredQuantity);
    }
}

public sealed class ListInventoryItemsQueryHandler(IInventoryRepository repository)
    : IRequestHandler<ListInventoryItemsQuery, IReadOnlyCollection<InventoryItemDto>>
{
    public async Task<IReadOnlyCollection<InventoryItemDto>> Handle(ListInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await repository.ListAsync(cancellationToken);
        return items
            .Select(x => new InventoryItemDto(x.Id, x.Sku, x.QuantityOnHand, x.ReservedQuantity, x.AvailableQuantity))
            .ToArray();
    }
}
