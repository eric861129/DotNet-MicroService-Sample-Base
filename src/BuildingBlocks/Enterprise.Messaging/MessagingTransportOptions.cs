namespace Enterprise.Messaging;

public sealed class MessagingTransportOptions
{
    public const string SectionName = "Messaging";

    public string Provider { get; init; } = "RabbitMq";

    public RabbitMqOptions RabbitMq { get; init; } = new();

    public ServiceBusOptions ServiceBus { get; init; } = new();

    public sealed class RabbitMqOptions
    {
        public string Host { get; init; } = "localhost";

        public ushort Port { get; init; } = 5672;

        public string VirtualHost { get; init; } = "/";

        public string Username { get; init; } = "guest";

        public string Password { get; init; } = "guest";
    }

    public sealed class ServiceBusOptions
    {
        public string ConnectionString { get; init; } = string.Empty;
    }
}
