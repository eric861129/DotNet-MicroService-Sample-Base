using FluentValidation;
using MediatR;

namespace Enterprise.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // 沒有驗證器時就直接放行，避免做無意義的檢查。
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(validators.Select(x => x.ValidateAsync(context, cancellationToken)));
        var failures = results
            .SelectMany(x => x.Errors)
            .Where(x => x is not null)
            .ToList();

        // 把所有錯誤一次收集起來，使用者就能一次修完，不用一個一個撞牆。
        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
