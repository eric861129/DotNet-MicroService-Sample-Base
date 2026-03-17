namespace OrderingService.Api.Catalog;

public sealed class CatalogServiceUnavailableException(string message) : Exception(message)
{
}
