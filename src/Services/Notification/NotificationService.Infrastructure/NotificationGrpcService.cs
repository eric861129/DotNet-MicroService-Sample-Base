using Grpc.Core;
using MediatR;
using NotificationService.Application;
using NotificationService.Contracts;

namespace NotificationService.Infrastructure;

public sealed class NotificationGrpcService(IMediator mediator) : Contracts.NotificationGrpc.NotificationGrpcBase
{
    public override async Task<GetRecentNotificationsReply> GetRecentNotifications(GetRecentNotificationsRequest request, ServerCallContext context)
    {
        var notifications = await mediator.Send(new GetRecentNotificationsQuery(), context.CancellationToken);
        var reply = new GetRecentNotificationsReply();

        // 這裡把內部 DTO 轉成 gRPC reply，讓其他服務可以安全讀取通知資訊。
        reply.Notifications.AddRange(notifications.Select(x => new NotificationReply
        {
            NotificationId = x.NotificationId.ToString(),
            OrderId = x.OrderId.ToString(),
            Recipient = x.Recipient,
            Message = x.Message,
            CreatedAtUtc = x.CreatedAtUtc.ToString("O")
        }));

        return reply;
    }
}
