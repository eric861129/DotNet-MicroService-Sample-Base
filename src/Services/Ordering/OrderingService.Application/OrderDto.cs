namespace OrderingService.Application;

public sealed record OrderItemDto(Guid ProductId, string Sku, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);

public sealed record OrderDto(Guid OrderId, string CustomerEmail, string Status, decimal TotalAmount, IReadOnlyCollection<OrderItemDto> Items);
