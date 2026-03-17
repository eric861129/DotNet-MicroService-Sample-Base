using Enterprise.SharedKernel.Domain;

namespace NotificationService.Domain;

public sealed class NotificationLog : AggregateRoot
{
    private NotificationLog()
    {
    }

    private NotificationLog(Guid orderId, string recipient, string message)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Recipient = string.IsNullOrWhiteSpace(recipient) ? throw new DomainException("收件人不可為空。") : recipient.Trim();
        Message = string.IsNullOrWhiteSpace(message) ? throw new DomainException("通知內容不可為空。") : message.Trim();
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid OrderId { get; private set; }

    public string Recipient { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public static NotificationLog Create(Guid orderId, string recipient, string message)
    {
        // 通知紀錄建立後就視為歷史資料，不提供隨意修改內容。
        return new NotificationLog(orderId, recipient, message);
    }
}
