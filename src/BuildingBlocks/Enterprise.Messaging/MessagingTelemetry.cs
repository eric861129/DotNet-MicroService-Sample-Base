using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Enterprise.Messaging;

public static class MessagingTelemetry
{
    public const string MeterName = "Enterprise.Messaging";

    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> OutboxEnqueued = Meter.CreateCounter<long>("enterprise_outbox_enqueued", unit: "{event}");
    private static readonly Counter<long> OutboxPublished = Meter.CreateCounter<long>("enterprise_outbox_published", unit: "{event}");
    private static readonly Counter<long> OutboxFailed = Meter.CreateCounter<long>("enterprise_outbox_failed", unit: "{event}");
    private static readonly Histogram<double> OutboxDispatchDuration = Meter.CreateHistogram<double>("enterprise_outbox_dispatch_duration", unit: "s");
    private static readonly Counter<long> ConsumerProcessed = Meter.CreateCounter<long>("enterprise_consumer_processed", unit: "{event}");
    private static readonly Counter<long> ConsumerDuplicates = Meter.CreateCounter<long>("enterprise_consumer_duplicate", unit: "{event}");
    private static readonly Counter<long> ConsumerFailures = Meter.CreateCounter<long>("enterprise_consumer_failure", unit: "{event}");
    private static readonly Counter<long> ConsumerRetries = Meter.CreateCounter<long>("enterprise_consumer_retry", unit: "{event}");

    public static void RecordOutboxEnqueued(string owner, string eventType) => OutboxEnqueued.Add(1, CreateTags(owner, eventType));
    public static void RecordOutboxPublished(string owner, string eventType) => OutboxPublished.Add(1, CreateTags(owner, eventType));
    public static void RecordOutboxFailed(string owner, string eventType) => OutboxFailed.Add(1, CreateTags(owner, eventType));
    public static void RecordOutboxDispatchDuration(string owner, string eventType, TimeSpan duration) => OutboxDispatchDuration.Record(duration.TotalSeconds, CreateTags(owner, eventType));
    public static void RecordConsumerProcessed(string owner, string eventType) => ConsumerProcessed.Add(1, CreateTags(owner, eventType));
    public static void RecordConsumerDuplicate(string owner, string eventType) => ConsumerDuplicates.Add(1, CreateTags(owner, eventType));
    public static void RecordConsumerFailure(string owner, string eventType) => ConsumerFailures.Add(1, CreateTags(owner, eventType));

    public static void RecordConsumerRetry(string owner, string eventType, int retryAttempt)
    {
        if (retryAttempt <= 0)
        {
            return;
        }

        ConsumerRetries.Add(retryAttempt, CreateTags(owner, eventType));
    }

    private static TagList CreateTags(string owner, string eventType)
    {
        var tags = new TagList
        {
            { "service_name", owner },
            { "event_type", eventType }
        };

        return tags;
    }
}
