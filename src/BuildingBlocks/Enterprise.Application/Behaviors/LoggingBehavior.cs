using MediatR;
using Microsoft.Extensions.Logging;

namespace Enterprise.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // 每個進到 Application 的請求都先寫開始 log，
        // 方便之後查是誰進來、帶了什麼資料。
        logger.LogInformation("開始處理應用程式請求 {RequestName}: {@Request}", typeof(TRequest).Name, request);

        var response = await next();

        // 成功完成後再補一筆結束 log，整段流程才完整。
        logger.LogInformation("完成處理應用程式請求 {RequestName}", typeof(TRequest).Name);

        return response;
    }
}
