namespace CatalogService.Api;

public sealed class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Sku { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public static Product Create(string sku, string name, decimal price)
    {
        return new Product
        {
            Sku = sku.Trim(),
            Name = name.Trim(),
            Price = decimal.Round(price, 2, MidpointRounding.AwayFromZero)
        };
    }

    public ProductResponse ToResponse() => new(Id, Sku, Name, Price);
}
