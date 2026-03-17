using Enterprise.Application.Abstractions;
using MediatR;

namespace NotificationService.Application;

public sealed record GetRecentNotificationsQuery() : IQuery<IReadOnlyCollection<NotificationLogDto>>;

public sealed class GetRecentNotificationsQueryHandler(INotificationRepository repository)
    : IRequestHandler<GetRecentNotificationsQuery, IReadOnlyCollection<NotificationLogDto>>
{
    public async Task<IReadOnlyCollection<NotificationLogDto>> Handle(GetRecentNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await repository.ListAsync(cancellationToken);
        return notifications
            .Select(x => new NotificationLogDto(x.Id, x.OrderId, x.Recipient, x.Message, x.CreatedAtUtc))
            .ToArray();
    }
}
