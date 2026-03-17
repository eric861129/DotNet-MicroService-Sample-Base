namespace InventoryService.Application;

public sealed record InventoryItemDto(Guid ProductId, string Sku, int QuantityOnHand, int ReservedQuantity, int AvailableQuantity);
