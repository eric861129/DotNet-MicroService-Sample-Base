namespace OrderingService.Api;

public sealed record OrderResponse(
    Guid OrderId,
    string CustomerEmail,
    decimal TotalAmount,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyCollection<OrderItemResponse> Items);

public sealed record OrderItemResponse(
    Guid ProductId,
    string Sku,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);
