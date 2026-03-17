using Microsoft.EntityFrameworkCore;
using NotificationService.Application;
using NotificationService.Domain;

namespace NotificationService.Infrastructure;

public sealed class NotificationRepository(NotificationDbContext dbContext) : INotificationRepository
{
    public Task AddAsync(NotificationLog notification, CancellationToken cancellationToken = default)
    {
        dbContext.Notifications.Add(notification);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyCollection<NotificationLog>> ListAsync(CancellationToken cancellationToken = default)
    {
        // 只取最近 50 筆，避免示範 API 一次把所有歷史資料都撈出來。
        return await dbContext.Notifications
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(50)
            .ToListAsync(cancellationToken);
    }
}
