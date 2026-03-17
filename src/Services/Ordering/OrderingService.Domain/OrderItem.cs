using Enterprise.SharedKernel.Domain;

namespace OrderingService.Domain;

public sealed class OrderItem : Entity
{
    private OrderItem()
    {
    }

    public OrderItem(Guid productId, string sku, string productName, decimal unitPrice, int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("訂單項目數量必須大於 0。");
        }

        Id = Guid.NewGuid();
        ProductId = productId;
        Sku = sku;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public Guid ProductId { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public string ProductName { get; private set; } = string.Empty;

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    // 單一品項總價由單價 * 數量算出來，
    // 這樣每個項目本身就知道自己值多少錢。
    public decimal LineTotal => decimal.Round(UnitPrice * Quantity, 2, MidpointRounding.AwayFromZero);
}
