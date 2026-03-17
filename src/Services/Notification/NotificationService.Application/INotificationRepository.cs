using NotificationService.Domain;

namespace NotificationService.Application;

public interface INotificationRepository
{
    Task AddAsync(NotificationLog notification, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<NotificationLog>> ListAsync(CancellationToken cancellationToken = default);
}
