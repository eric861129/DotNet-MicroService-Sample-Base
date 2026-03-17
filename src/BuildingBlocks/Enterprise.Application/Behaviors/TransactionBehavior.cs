using Enterprise.Application.Abstractions;
using MediatR;

namespace Enterprise.Application.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        // 只有 Command 才需要存檔，
        // 因為 Query 的責任只是查資料，不應該偷偷改狀態。
        if (request is ICommandBase)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
