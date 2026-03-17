using Enterprise.Application.Abstractions;
using MediatR;

namespace OrderingService.Application;

public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto?>;

public sealed class GetOrderByIdQueryHandler(IOrderRepository repository)
    : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        return new OrderDto(
            order.Id,
            order.CustomerEmail,
            order.Status.ToString(),
            order.TotalAmount,
            order.Items.Select(x => new OrderItemDto(x.ProductId, x.Sku, x.ProductName, x.UnitPrice, x.Quantity, x.LineTotal)).ToArray());
    }
}
