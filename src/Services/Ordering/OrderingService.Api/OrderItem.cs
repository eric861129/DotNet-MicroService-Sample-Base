namespace OrderingService.Api;

public sealed class OrderItem
{
    public Guid ProductId { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public string ProductName { get; private set; } = string.Empty;

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public decimal LineTotal { get; private set; }

    public static OrderItem Create(Guid productId, string sku, string productName, decimal unitPrice, int quantity)
    {
        return new OrderItem
        {
            ProductId = productId,
            Sku = sku,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity,
            LineTotal = unitPrice * quantity
        };
    }

    public OrderItemResponse ToResponse() => new(ProductId, Sku, ProductName, UnitPrice, Quantity, LineTotal);
}
