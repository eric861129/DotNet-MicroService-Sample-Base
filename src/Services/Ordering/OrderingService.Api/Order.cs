namespace OrderingService.Api;

public sealed class Order
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string CustomerEmail { get; private set; } = string.Empty;

    public decimal TotalAmount { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    public List<OrderItem> Items { get; private set; } = [];

    public static Order Create(string customerEmail, IReadOnlyCollection<OrderItem> items)
    {
        return new Order
        {
            CustomerEmail = customerEmail.Trim(),
            Items = items.ToList(),
            TotalAmount = items.Sum(item => item.LineTotal)
        };
    }

    public OrderResponse ToResponse() => new(
        Id,
        CustomerEmail,
        TotalAmount,
        CreatedAtUtc,
        Items.Select(item => item.ToResponse()).ToArray());
}
