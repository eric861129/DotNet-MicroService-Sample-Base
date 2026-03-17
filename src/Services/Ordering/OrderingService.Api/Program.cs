using Enterprise.Application;
using Enterprise.Configuration;
using Enterprise.Messaging;
using Enterprise.Observability;
using Enterprise.Persistence;
using Enterprise.Security;
using Enterprise.ServiceDefaults;
using MediatR;
using OrderingService.Application;
using OrderingService.Contracts;
using OrderingService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseConfiguration();
builder.AddEnterpriseObservability("OrderingService");

// Ordering 是這個範例的主角：
// 它會做同步查詢、寫訂單、寫 Outbox，最後再由背景工作發事件。
builder.Services.AddEnterpriseServiceDefaults(builder.Configuration);
builder.Services.AddEnterpriseJwtAuthentication(builder.Configuration);
builder.Services.AddEnterpriseApplication(typeof(PlaceOrderCommand).Assembly);
builder.Services.AddOrderingInfrastructure(builder.Configuration);
builder.Services.AddEnterpriseEventTypeRegistry(typeof(OrderPlacedIntegrationEvent).Assembly);
builder.Services.AddEnterpriseMassTransit(builder.Configuration);
builder.Services.AddGrpc();
builder.Services.AddOpenApi();
builder.Services.AddHostedService<DatabaseMigrationHostedService<OrderingDbContext>>();

// Outbox dispatcher 負責把待發事件慢慢送出去，
// 這樣下單交易就不會跟訊息代理是否當下可用綁死。
builder.Services.AddHostedService<OutboxDispatcherBackgroundService<OrderingDbContext>>();

var app = builder.Build();

app.UseEnterpriseServiceDefaults();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new { Service = "OrderingService", Status = "Running" }));
app.MapGet("/api/orders/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
{
    var order = await sender.Send(new GetOrderByIdQuery(id), cancellationToken);
    return order is null ? Results.NotFound() : Results.Ok(order);
});
app.MapPost("/api/orders", async (PlaceOrderCommand command, ISender sender, CancellationToken cancellationToken) =>
{
    var created = await sender.Send(command, cancellationToken);
    return Results.Created($"/api/orders/{created.OrderId}", created);
});

// 內部服務若想查訂單，可以走 gRPC。
app.MapGrpcService<OrderingGrpcService>();

app.Run();

public partial class Program;
