using Enterprise.SharedKernel.Domain;

namespace InventoryService.Domain;

public sealed class InventoryItem : AggregateRoot
{
    private InventoryItem()
    {
    }

    private InventoryItem(Guid productId, string sku, int quantityOnHand)
    {
        Id = productId;
        Sku = string.IsNullOrWhiteSpace(sku) ? throw new DomainException("SKU 不可為空。") : sku.Trim();
        if (quantityOnHand < 0)
        {
            throw new DomainException("庫存數量不可小於 0。");
        }

        QuantityOnHand = quantityOnHand;
    }

    public string Sku { get; private set; } = string.Empty;

    public int QuantityOnHand { get; private set; }

    public int ReservedQuantity { get; private set; }

    // AvailableQuantity = 真正還能賣出去的數量。
    public int AvailableQuantity => QuantityOnHand - ReservedQuantity;

    public static InventoryItem Create(Guid productId, string sku, int quantityOnHand)
    {
        return new InventoryItem(productId, sku, quantityOnHand);
    }

    public void Restock(int quantity)
    {
        if (quantity < 0)
        {
            throw new DomainException("補貨數量不可小於 0。");
        }

        // 這個範例採「覆蓋庫存量」而不是「累加補貨量」的做法。
        QuantityOnHand = quantity;
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("保留數量必須大於 0。");
        }

        if (AvailableQuantity < quantity)
        {
            throw new DomainException("庫存不足。");
        }

        // 保留量增加之後，可用量就會自然下降。
        ReservedQuantity += quantity;
    }
}
