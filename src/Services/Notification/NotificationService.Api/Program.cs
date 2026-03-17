using Enterprise.Application;
using Enterprise.Configuration;
using Enterprise.Messaging;
using Enterprise.Observability;
using Enterprise.Persistence;
using Enterprise.Security;
using Enterprise.ServiceDefaults;
using MediatR;
using NotificationService.Application;
using NotificationService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseConfiguration();
builder.AddEnterpriseObservability("NotificationService");

builder.Services.AddEnterpriseServiceDefaults(builder.Configuration);
builder.Services.AddEnterpriseJwtAuthentication(builder.Configuration);
builder.Services.AddEnterpriseApplication(typeof(GetRecentNotificationsQuery).Assembly);
builder.Services.AddNotificationInfrastructure(builder.Configuration);
builder.Services.AddEnterpriseMassTransit(builder.Configuration, registration =>
{
    // Notification 不直接參與下單，但會聽事件來建立通知紀錄。
    registration.AddConsumer<OrderPlacedConsumer>();
});
builder.Services.AddGrpc();
builder.Services.AddOpenApi();
builder.Services.AddHostedService<DatabaseMigrationHostedService<NotificationDbContext>>();

var app = builder.Build();

app.UseEnterpriseServiceDefaults();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new { Service = "NotificationService", Status = "Running" }));
app.MapGet("/api/notifications", async (ISender sender, CancellationToken cancellationToken) =>
    Results.Ok(await sender.Send(new GetRecentNotificationsQuery(), cancellationToken)));

// gRPC 入口保留給內部服務查近期通知。
app.MapGrpcService<NotificationGrpcService>();

app.Run();

public partial class Program;
