using FluentAssertions;
using OrderingService.Domain;

namespace Enterprise.UnitTests;

public sealed class OrderingDomainTests
{
    [Fact]
    public void Place_should_calculate_total_amount_from_all_order_items()
    {
        var order = Order.Place(
            "buyer@example.com",
            [
                new OrderItem(Guid.NewGuid(), "SKU-001", "Keyboard", 1200m, 2),
                new OrderItem(Guid.NewGuid(), "SKU-002", "Mouse", 800m, 1)
            ]);

        order.TotalAmount.Should().Be(3200m);
        order.Items.Should().HaveCount(2);
        order.Status.Should().Be(OrderStatus.Pending);
    }
}
