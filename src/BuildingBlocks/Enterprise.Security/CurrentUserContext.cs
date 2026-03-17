using System.Security.Claims;
using Enterprise.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Enterprise.Security;

public sealed class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    // 這個類別像翻譯員，把 HttpContext 裡的 Claims 轉成 Application 層看得懂的格式。
    public string SubjectId => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

    public string? DisplayName => httpContextAccessor.HttpContext?.User.Identity?.Name;

    public IReadOnlyCollection<string> Roles =>
        httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray()
        ?? Array.Empty<string>();

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
