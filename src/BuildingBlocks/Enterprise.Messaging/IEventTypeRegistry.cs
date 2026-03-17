namespace Enterprise.Messaging;

public interface IEventTypeRegistry
{
    Type Resolve(string eventType);
}
