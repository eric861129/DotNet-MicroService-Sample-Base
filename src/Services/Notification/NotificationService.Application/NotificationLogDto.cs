namespace NotificationService.Application;

public sealed record NotificationLogDto(Guid NotificationId, Guid OrderId, string Recipient, string Message, DateTime CreatedAtUtc);
