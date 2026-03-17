using Enterprise.SharedKernel.Domain;

namespace CatalogService.Domain;

public sealed class Product : AggregateRoot
{
    private Product()
    {
    }

    private Product(Guid id, string sku, string name, decimal price)
    {
        Id = id;
        Sku = string.IsNullOrWhiteSpace(sku) ? throw new DomainException("SKU 不可為空。") : sku.Trim();
        Name = string.IsNullOrWhiteSpace(name) ? throw new DomainException("商品名稱不可為空。") : name.Trim();
        SetPrice(price);
    }

    public string Sku { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public static Product Create(string sku, string name, decimal price)
    {
        // 建立商品時不讓外部直接 new，
        // 這樣可以強迫所有建立流程都經過同一套商業規則檢查。
        return new Product(Guid.NewGuid(), sku, name, price);
    }

    public void Rename(string name)
    {
        // 商品名稱是業務資料，不應該接受空白字串。
        Name = string.IsNullOrWhiteSpace(name) ? throw new DomainException("商品名稱不可為空。") : name.Trim();
    }

    public void SetPrice(decimal price)
    {
        // 價格不能小於等於 0，這是 Domain 層自己保護的規則。
        if (price <= 0)
        {
            throw new DomainException("商品價格必須大於 0。");
        }

        // 統一在這裡處理小數位，避免不同地方各自決定四捨五入方式。
        Price = decimal.Round(price, 2, MidpointRounding.AwayFromZero);
    }
}
