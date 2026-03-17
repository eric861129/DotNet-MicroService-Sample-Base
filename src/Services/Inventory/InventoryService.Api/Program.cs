using Enterprise.Application;
using Enterprise.Configuration;
using Enterprise.Messaging;
using Enterprise.Observability;
using Enterprise.Persistence;
using Enterprise.Security;
using Enterprise.ServiceDefaults;
using InventoryService.Application;
using InventoryService.Infrastructure;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseConfiguration();
builder.AddEnterpriseObservability("InventoryService");

builder.Services.AddEnterpriseServiceDefaults(builder.Configuration);
builder.Services.AddEnterpriseJwtAuthentication(builder.Configuration);
builder.Services.AddEnterpriseApplication(typeof(UpsertInventoryItemCommand).Assembly);
builder.Services.AddInventoryInfrastructure(builder.Configuration);
builder.Services.AddEnterpriseMassTransit(builder.Configuration, registration =>
{
    // Inventory 會聽訂單成立事件，因為它要根據訂單保留庫存。
    registration.AddConsumer<OrderPlacedConsumer>();
});
builder.Services.AddGrpc();
builder.Services.AddOpenApi();
builder.Services.AddHostedService<DatabaseMigrationHostedService<InventoryDbContext>>();

var app = builder.Build();

app.UseEnterpriseServiceDefaults();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new { Service = "InventoryService", Status = "Running" }));
app.MapGet("/api/inventory", async (ISender sender, CancellationToken cancellationToken) =>
    Results.Ok(await sender.Send(new ListInventoryItemsQuery(), cancellationToken)));
app.MapGet("/api/inventory/{productId:guid}", async (Guid productId, int quantity, ISender sender, CancellationToken cancellationToken) =>
    Results.Ok(await sender.Send(new GetInventoryAvailabilityQuery(productId, quantity <= 0 ? 1 : quantity), cancellationToken)));
app.MapPost("/api/inventory", async (UpsertInventoryItemCommand command, ISender sender, CancellationToken cancellationToken) =>
{
    var result = await sender.Send(command, cancellationToken);
    return Results.Ok(result);
});

// gRPC 主要是給 Ordering 下單前同步問「庫存夠不夠」。
app.MapGrpcService<InventoryGrpcService>();

app.Run();

public partial class Program;
