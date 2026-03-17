using Enterprise.Messaging;
using Enterprise.Persistence;
using FluentAssertions;
using InventoryService.Domain;
using InventoryService.Infrastructure;
using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NotificationService.Infrastructure;
using OrderingService.Application;
using OrderingService.Contracts;
using OrderingService.Infrastructure;
using Testcontainers.MsSql;

namespace Enterprise.IntegrationTests;

public sealed class OrderProcessingEndToEndTests
{
    [Fact]
    public async Task Placing_an_order_should_dispatch_outbox_and_keep_consumers_idempotent()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RUN_CONTAINER_TESTS"), "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await using var sql = new MsSqlBuilder().Build();
        await sql.StartAsync();

        var productId = Guid.NewGuid();
        var orderingConnectionString = BuildConnectionString(sql.GetConnectionString(), "OrderingE2E");
        var inventoryConnectionString = BuildConnectionString(sql.GetConnectionString(), "InventoryE2E");
        var notificationConnectionString = BuildConnectionString(sql.GetConnectionString(), "NotificationE2E");

        var inventoryServices = new ServiceCollection();
        inventoryServices.AddLogging();
        inventoryServices.AddDbContext<InventoryDbContext>(options => options.UseSqlServer(inventoryConnectionString));
        inventoryServices.AddScoped<IInboxStore>(provider => provider.GetRequiredService<InventoryDbContext>());
        var inventoryProvider = inventoryServices.BuildServiceProvider();

        var notificationServices = new ServiceCollection();
        notificationServices.AddLogging();
        notificationServices.AddDbContext<NotificationDbContext>(options => options.UseSqlServer(notificationConnectionString));
        notificationServices.AddScoped<IInboxStore>(provider => provider.GetRequiredService<NotificationDbContext>());
        var notificationProvider = notificationServices.BuildServiceProvider();

        var publishedEvents = new List<IIntegrationEvent>();
        var publishEndpoint = new RecordingPublishEndpoint(async (message, cancellationToken) =>
        {
            publishedEvents.Add(message);

            if (message is not OrderPlacedIntegrationEvent orderPlaced)
            {
                return;
            }

            using var inventoryScope = inventoryProvider.CreateScope();
            var inventoryConsumer = new InventoryService.Infrastructure.OrderPlacedConsumer(
                inventoryScope.ServiceProvider.GetRequiredService<InventoryDbContext>(),
                inventoryScope.ServiceProvider.GetRequiredService<IInboxStore>(),
                NullLogger<InventoryService.Infrastructure.OrderPlacedConsumer>.Instance);
            await inventoryConsumer.ConsumeEventAsync(orderPlaced, cancellationToken);

            using var notificationScope = notificationProvider.CreateScope();
            var notificationConsumer = new NotificationService.Infrastructure.OrderPlacedConsumer(
                notificationScope.ServiceProvider.GetRequiredService<NotificationDbContext>(),
                notificationScope.ServiceProvider.GetRequiredService<IInboxStore>(),
                NullLogger<NotificationService.Infrastructure.OrderPlacedConsumer>.Instance);
            await notificationConsumer.ConsumeEventAsync(orderPlaced, cancellationToken);
        });

        var orderingServices = new ServiceCollection();
        orderingServices.AddLogging();
        orderingServices.AddDbContext<OrderingDbContext>(options => options.UseSqlServer(orderingConnectionString));
        orderingServices.AddScoped<IOrderRepository, OrderRepository>();
        orderingServices.AddScoped<IOutboxStore>(provider => provider.GetRequiredService<OrderingDbContext>());
        orderingServices.AddSingleton<IEventTypeRegistry>(_ => new EventTypeRegistry([typeof(OrderPlacedIntegrationEvent).Assembly]));
        orderingServices.AddSingleton<IPublishEndpoint>(publishEndpoint);
        var orderingProvider = orderingServices.BuildServiceProvider();

        await EnsureDatabaseCreatedAsync<InventoryDbContext>(inventoryProvider);
        await EnsureDatabaseCreatedAsync<NotificationDbContext>(notificationProvider);
        await EnsureDatabaseCreatedAsync<OrderingDbContext>(orderingProvider);

        using (var inventorySeedScope = inventoryProvider.CreateScope())
        {
            var inventoryDb = inventorySeedScope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            inventoryDb.InventoryItems.Add(InventoryItem.Create(productId, "SKU-DEMO-001", 10));
            await inventoryDb.SaveChangesAsync();
        }

        OrderDto createdOrder;
        using (var orderingScope = orderingProvider.CreateScope())
        {
            var orderingDb = orderingScope.ServiceProvider.GetRequiredService<OrderingDbContext>();
            var repository = new OrderRepository(orderingDb);
            var handler = new PlaceOrderCommandHandler(
                repository,
                new FakeProductCatalogClient(productId),
                new FakeInventoryAvailabilityClient(productId, 10),
                orderingDb);

            createdOrder = await handler.Handle(
                new PlaceOrderCommand("student@example.com", [new PlaceOrderItemRequest(productId, 2)]),
                CancellationToken.None);

            await orderingDb.SaveChangesAsync();
        }

        using (var assertOrderingScope = orderingProvider.CreateScope())
        {
            var orderingDb = assertOrderingScope.ServiceProvider.GetRequiredService<OrderingDbContext>();
            var pendingOutbox = await orderingDb.OutboxMessages.SingleAsync();
            pendingOutbox.ProcessedOnUtc.Should().BeNull();
            createdOrder.Items.Should().ContainSingle();
        }

        var dispatcher = new OutboxDispatcherBackgroundService<OrderingDbContext>(
            orderingProvider.GetRequiredService<IServiceScopeFactory>(),
            orderingProvider.GetRequiredService<IEventTypeRegistry>(),
            NullLogger<OutboxDispatcherBackgroundService<OrderingDbContext>>.Instance);

        await dispatcher.ProcessBatchOnceAsync();

        publishedEvents.Should().ContainSingle();
        var publishedOrderEvent = publishedEvents.Single().Should().BeOfType<OrderPlacedIntegrationEvent>().Subject;
        publishedOrderEvent.OrderId.Should().Be(createdOrder.OrderId);

        using (var assertOrderingScope = orderingProvider.CreateScope())
        {
            var orderingDb = assertOrderingScope.ServiceProvider.GetRequiredService<OrderingDbContext>();
            var processedOutbox = await orderingDb.OutboxMessages.SingleAsync();
            processedOutbox.ProcessedOnUtc.Should().NotBeNull();
            processedOutbox.Error.Should().BeNull();
        }

        using (var inventoryAssertScope = inventoryProvider.CreateScope())
        {
            var inventoryDb = inventoryAssertScope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var inventoryItem = await inventoryDb.InventoryItems.SingleAsync();
            inventoryItem.ReservedQuantity.Should().Be(2);
            inventoryItem.AvailableQuantity.Should().Be(8);
            inventoryDb.InboxMessages.Should().ContainSingle();
        }

        using (var notificationAssertScope = notificationProvider.CreateScope())
        {
            var notificationDb = notificationAssertScope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            var notifications = await notificationDb.Notifications.ToListAsync();
            notifications.Should().ContainSingle();
            notificationDb.InboxMessages.Should().ContainSingle();
        }

        using (var inventoryDuplicateScope = inventoryProvider.CreateScope())
        {
            var inventoryConsumer = new InventoryService.Infrastructure.OrderPlacedConsumer(
                inventoryDuplicateScope.ServiceProvider.GetRequiredService<InventoryDbContext>(),
                inventoryDuplicateScope.ServiceProvider.GetRequiredService<IInboxStore>(),
                NullLogger<InventoryService.Infrastructure.OrderPlacedConsumer>.Instance);
            await inventoryConsumer.ConsumeEventAsync(publishedOrderEvent, retryAttempt: 1);
        }

        using (var notificationDuplicateScope = notificationProvider.CreateScope())
        {
            var notificationConsumer = new NotificationService.Infrastructure.OrderPlacedConsumer(
                notificationDuplicateScope.ServiceProvider.GetRequiredService<NotificationDbContext>(),
                notificationDuplicateScope.ServiceProvider.GetRequiredService<IInboxStore>(),
                NullLogger<NotificationService.Infrastructure.OrderPlacedConsumer>.Instance);
            await notificationConsumer.ConsumeEventAsync(publishedOrderEvent, retryAttempt: 1);
        }

        using (var inventoryAssertScope = inventoryProvider.CreateScope())
        {
            var inventoryDb = inventoryAssertScope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var inventoryItem = await inventoryDb.InventoryItems.SingleAsync();
            inventoryItem.ReservedQuantity.Should().Be(2);
            inventoryDb.InboxMessages.Should().ContainSingle();
        }

        using (var notificationAssertScope = notificationProvider.CreateScope())
        {
            var notificationDb = notificationAssertScope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            (await notificationDb.Notifications.CountAsync()).Should().Be(1);
            notificationDb.InboxMessages.Should().ContainSingle();
        }
    }

    private static async Task EnsureDatabaseCreatedAsync<TDbContext>(IServiceProvider provider)
        where TDbContext : DbContext
    {
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    private static string BuildConnectionString(string baseConnectionString, string databaseName)
    {
        var builder = new SqlConnectionStringBuilder(baseConnectionString)
        {
            InitialCatalog = databaseName
        };

        return builder.ConnectionString;
    }

    private sealed class FakeProductCatalogClient(Guid productId) : IProductCatalogClient
    {
        public Task<CatalogProductSnapshot?> GetProductAsync(Guid requestedProductId, CancellationToken cancellationToken = default)
        {
            var product = requestedProductId == productId
                ? new CatalogProductSnapshot(productId, "SKU-DEMO-001", "Demo Product", 1280m)
                : null;

            return Task.FromResult(product);
        }
    }

    private sealed class FakeInventoryAvailabilityClient(Guid productId, int availableQuantity) : IInventoryAvailabilityClient
    {
        public Task<InventoryAvailabilitySnapshot> CheckAvailabilityAsync(Guid requestedProductId, int quantity, CancellationToken cancellationToken = default)
        {
            var isAvailable = requestedProductId == productId && availableQuantity >= quantity;
            return Task.FromResult(new InventoryAvailabilitySnapshot(requestedProductId, availableQuantity, isAvailable));
        }
    }

    private sealed class RecordingPublishEndpoint(Func<IIntegrationEvent, CancellationToken, Task> dispatcher) : IPublishEndpoint
    {
        public ConnectHandle ConnectPublishObserver(IPublishObserver observer) => throw new NotSupportedException();

        public Task Publish<T>(T message, CancellationToken cancellationToken = default) where T : class
            => dispatcher((IIntegrationEvent)(object)message, cancellationToken);

        public Task Publish<T>(T message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class
            => Publish(message, cancellationToken);

        public Task Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class
            => Publish(message, cancellationToken);

        public Task Publish(object message, CancellationToken cancellationToken = default)
            => dispatcher((IIntegrationEvent)message, cancellationToken);

        public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
            => dispatcher((IIntegrationEvent)message, cancellationToken);

        public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default)
            => dispatcher((IIntegrationEvent)message, cancellationToken);

        public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default)
            => Publish(message, messageType, cancellationToken);

        public Task Publish<T>(object values, CancellationToken cancellationToken = default) where T : class
            => throw new NotSupportedException();

        public Task Publish<T>(object values, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class
            => throw new NotSupportedException();

        public Task Publish<T>(object values, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class
            => throw new NotSupportedException();
    }
}
