namespace CatalogService.Api;

public sealed record CreateProductRequest(string Sku, string Name, decimal Price)
{
    public Dictionary<string, string[]> Validate()
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(Sku))
        {
            errors["sku"] = ["SKU is required."];
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            errors["name"] = ["Name is required."];
        }

        if (Price <= 0)
        {
            errors["price"] = ["Price must be greater than zero."];
        }

        return errors;
    }
}
