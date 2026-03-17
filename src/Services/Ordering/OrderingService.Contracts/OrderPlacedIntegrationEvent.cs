using Enterprise.Messaging;

namespace OrderingService.Contracts;

public sealed record OrderPlacedOrderItem(Guid ProductId, string Sku, string ProductName, int Quantity, decimal UnitPrice);

public sealed record OrderPlacedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }

    public string CustomerEmail { get; init; } = string.Empty;

    public decimal TotalAmount { get; init; }

    public IReadOnlyCollection<OrderPlacedOrderItem> Items { get; init; } = Array.Empty<OrderPlacedOrderItem>();
}
