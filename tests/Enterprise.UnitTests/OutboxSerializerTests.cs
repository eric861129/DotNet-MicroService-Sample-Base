using Enterprise.Messaging;
using FluentAssertions;
using OrderingService.Contracts;

namespace Enterprise.UnitTests;

public sealed class OutboxSerializerTests
{
    [Fact]
    public void Serialize_and_deserialize_should_keep_order_placed_event_payload()
    {
        var integrationEvent = new OrderPlacedIntegrationEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerEmail = "buyer@example.com",
            TotalAmount = 1999m,
            Items =
            [
                new OrderPlacedOrderItem(Guid.NewGuid(), "SKU-001", "Keyboard", 1, 1999m)
            ]
        };

        var message = OutboxSerializer.Serialize(integrationEvent);
        var registry = new EventTypeRegistry(typeof(OrderPlacedIntegrationEvent).Assembly);
        var restored = OutboxSerializer.Deserialize(message, registry);

        restored.Should().BeEquivalentTo(integrationEvent);
    }
}
