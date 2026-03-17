using Enterprise.SharedKernel.Domain;

namespace OrderingService.Domain;

public sealed class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = [];

    private Order()
    {
    }

    private Order(string customerEmail, IEnumerable<OrderItem> items)
    {
        Id = Guid.NewGuid();
        CustomerEmail = string.IsNullOrWhiteSpace(customerEmail)
            ? throw new DomainException("客戶 Email 不可為空。")
            : customerEmail.Trim();

        _items.AddRange(items);

        // 訂單至少要有一個品項，不然就不是一張真正的訂單。
        if (_items.Count == 0)
        {
            throw new DomainException("訂單至少需要一個商品。");
        }

        Status = OrderStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public string CustomerEmail { get; private set; } = string.Empty;

    public OrderStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // 總金額不直接存欄位，而是每次由訂單項目即時計算，
    // 這樣比較不容易出現「總金額忘記同步更新」的問題。
    public decimal TotalAmount => _items.Sum(x => x.LineTotal);

    public static Order Place(string customerEmail, IEnumerable<OrderItem> items)
    {
        return new Order(customerEmail, items);
    }

    public void Confirm()
    {
        // 這個範例先保留最簡單的狀態流轉。
        Status = OrderStatus.Confirmed;
    }
}
