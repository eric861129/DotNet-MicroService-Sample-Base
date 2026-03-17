using Enterprise.SharedKernel.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Enterprise.ServiceDefaults;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 這裡像客服總台，把不同例外翻成統一格式的 ProblemDetails。
        logger.LogError(exception, "發生未處理例外");

        var problem = exception switch
        {
            ValidationException validationException => CreateValidationProblem(validationException),
            DomainException domainException => CreateProblem(StatusCodes.Status400BadRequest, "領域規則錯誤", domainException.Message),
            UnauthorizedAccessException unauthorizedAccessException => CreateProblem(StatusCodes.Status401Unauthorized, "未授權", unauthorizedAccessException.Message),
            _ => CreateProblem(StatusCodes.Status500InternalServerError, "系統錯誤", "請聯絡系統管理員或稍後再試。")
        };

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }

    private static ValidationProblemDetails CreateValidationProblem(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "驗證失敗",
            Detail = "輸入資料未通過驗證。"
        };
    }

    private static ProblemDetails CreateProblem(int statusCode, string title, string detail)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };
    }
}
