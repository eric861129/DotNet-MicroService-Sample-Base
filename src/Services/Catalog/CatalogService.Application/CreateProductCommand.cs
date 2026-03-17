using Enterprise.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace CatalogService.Application;

public sealed record CreateProductCommand(string Sku, string Name, decimal Price) : ICommand<ProductDto>;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        // 建商品時先守住最基本規則，避免把明顯錯誤資料塞進系統。
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

public sealed class CreateProductCommandHandler(ICatalogProductRepository repository)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Domain 負責保護商業規則，Application 只負責把用例串起來。
        var product = Domain.Product.Create(request.Sku, request.Name, request.Price);
        await repository.AddAsync(product, cancellationToken);

        return new ProductDto(product.Id, product.Sku, product.Name, product.Price);
    }
}
