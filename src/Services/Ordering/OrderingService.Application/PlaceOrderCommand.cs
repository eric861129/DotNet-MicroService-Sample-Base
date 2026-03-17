using Enterprise.Application.Abstractions;
using Enterprise.Messaging;
using Enterprise.SharedKernel.Domain;
using FluentValidation;
using MediatR;
using OrderingService.Contracts;
using OrderingService.Domain;

namespace OrderingService.Application;

public sealed record PlaceOrderItemRequest(Guid ProductId, int Quantity);

public sealed record PlaceOrderCommand(string CustomerEmail, IReadOnlyCollection<PlaceOrderItemRequest> Items) : ICommand<OrderDto>;

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        // 下單前先把最基本的資料品質顧好。
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty();
            item.RuleFor(x => x.Quantity).GreaterThan(0);
        });
    }
}

public sealed class PlaceOrderCommandHandler(
    IOrderRepository repository,
    IProductCatalogClient productCatalogClient,
    IInventoryAvailabilityClient inventoryAvailabilityClient,
    IOutboxStore outboxStore)
    : IRequestHandler<PlaceOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var orderItems = new List<OrderItem>();
        var eventItems = new List<OrderPlacedOrderItem>();

        // 下單時先同步問兩個鄰居：
        // 1. Catalog：商品存在嗎、價格是多少
        // 2. Inventory：庫存夠不夠
        // 兩邊都沒問題，才真的建立訂單。
        foreach (var item in request.Items)
        {
            var product = await productCatalogClient.GetProductAsync(item.ProductId, cancellationToken)
                ?? throw new DomainException($"找不到商品: {item.ProductId}");

            var availability = await inventoryAvailabilityClient.CheckAvailabilityAsync(item.ProductId, item.Quantity, cancellationToken);
            if (!availability.IsAvailable)
            {
                throw new DomainException($"商品 {product.Name} 庫存不足，可用數量為 {availability.AvailableQuantity}。");
            }

            var orderItem = new OrderItem(product.ProductId, product.Sku, product.Name, product.Price, item.Quantity);
            orderItems.Add(orderItem);
            eventItems.Add(new OrderPlacedOrderItem(product.ProductId, product.Sku, product.Name, item.Quantity, product.Price));
        }

        var order = Order.Place(request.CustomerEmail, orderItems);
        await repository.AddAsync(order, cancellationToken);

        // 這裡先寫 Outbox，不直接 Publish。
        // 這樣就算 RabbitMQ / Service Bus 一時故障，訂單資料也不會和事件脫鉤。
        await outboxStore.AddAsync(new OrderPlacedIntegrationEvent
        {
            OrderId = order.Id,
            CustomerEmail = order.CustomerEmail,
            TotalAmount = order.TotalAmount,
            Items = eventItems
        }, cancellationToken);

        return new OrderDto(
            order.Id,
            order.CustomerEmail,
            order.Status.ToString(),
            order.TotalAmount,
            order.Items.Select(x => new OrderItemDto(x.ProductId, x.Sku, x.ProductName, x.UnitPrice, x.Quantity, x.LineTotal)).ToArray());
    }
}
