using System.Reflection;

namespace Enterprise.Messaging;

public sealed class EventTypeRegistry(params Assembly[] assemblies) : IEventTypeRegistry
{
    private readonly IReadOnlyDictionary<string, Type> _map = assemblies
        .SelectMany(x => x.GetTypes())
        .Where(x => typeof(IIntegrationEvent).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface)
        .ToDictionary(x => x.FullName!, x => x, StringComparer.Ordinal);

    public Type Resolve(string eventType)
    {
        if (_map.TryGetValue(eventType, out var type))
        {
            return type;
        }

        throw new InvalidOperationException($"找不到事件型別: {eventType}");
    }
}
