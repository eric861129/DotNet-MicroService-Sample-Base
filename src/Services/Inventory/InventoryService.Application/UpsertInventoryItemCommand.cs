using Enterprise.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace InventoryService.Application;

public sealed record UpsertInventoryItemCommand(Guid ProductId, string Sku, int QuantityOnHand) : ICommand<InventoryItemDto>;

public sealed class UpsertInventoryItemCommandValidator : AbstractValidator<UpsertInventoryItemCommand>
{
    public UpsertInventoryItemCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(64);
        RuleFor(x => x.QuantityOnHand).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpsertInventoryItemCommandHandler(IInventoryRepository repository)
    : IRequestHandler<UpsertInventoryItemCommand, InventoryItemDto>
{
    public async Task<InventoryItemDto> Handle(UpsertInventoryItemCommand request, CancellationToken cancellationToken)
    {
        // Upsert 的意思是：
        // 有這筆資料就更新，沒有就建立新資料。
        var item = await repository.GetByProductIdAsync(request.ProductId, cancellationToken);

        if (item is null)
        {
            item = Domain.InventoryItem.Create(request.ProductId, request.Sku, request.QuantityOnHand);
            await repository.AddAsync(item, cancellationToken);
        }
        else
        {
            // 這個範例把輸入量視為新的現有庫存量，不是額外加上去的補貨量。
            item.Restock(request.QuantityOnHand);
        }

        return new InventoryItemDto(item.Id, item.Sku, item.QuantityOnHand, item.ReservedQuantity, item.AvailableQuantity);
    }
}
