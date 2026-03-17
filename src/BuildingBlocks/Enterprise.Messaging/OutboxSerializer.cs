using System.Text.Json;

namespace Enterprise.Messaging;

public static class OutboxSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static OutboxMessage Serialize(IIntegrationEvent integrationEvent)
    {
        // 事件進 Outbox table 前要先轉成純文字，
        // 這樣資料庫才能安全保存「待寄出的信」。
        var eventType = integrationEvent.GetType();

        return new OutboxMessage
        {
            EventId = integrationEvent.EventId,
            EventType = eventType.FullName ?? eventType.Name,
            Payload = JsonSerializer.Serialize(integrationEvent, eventType, JsonOptions),
            Version = integrationEvent.Version,
            OccurredOnUtc = integrationEvent.OccurredOnUtc
        };
    }

    public static IIntegrationEvent Deserialize(OutboxMessage message, IEventTypeRegistry registry)
    {
        // 背景工作會把資料庫裡的文字還原成真正事件，
        // 然後再把它送到 RabbitMQ / Service Bus。
        var type = registry.Resolve(message.EventType);
        return (IIntegrationEvent)(JsonSerializer.Deserialize(message.Payload, type, JsonOptions)
            ?? throw new InvalidOperationException($"無法還原事件內容: {message.EventType}"));
    }
}
