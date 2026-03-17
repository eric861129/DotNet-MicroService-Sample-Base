using CatalogService.Application;
using CatalogService.Infrastructure;
using Enterprise.Application;
using Enterprise.Configuration;
using Enterprise.Observability;
using Enterprise.Persistence;
using Enterprise.Security;
using Enterprise.ServiceDefaults;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseConfiguration();
builder.AddEnterpriseObservability("CatalogService");

// Catalog 是最容易入門的服務：
// 外部可用 REST 建商品，內部可用 gRPC 查商品。
builder.Services.AddEnterpriseServiceDefaults(builder.Configuration);
builder.Services.AddEnterpriseJwtAuthentication(builder.Configuration);
builder.Services.AddEnterpriseApplication(typeof(CreateProductCommand).Assembly);
builder.Services.AddCatalogInfrastructure(builder.Configuration);
builder.Services.AddGrpc();
builder.Services.AddOpenApi();
builder.Services.AddHostedService<DatabaseMigrationHostedService<CatalogDbContext>>();

var app = builder.Build();

app.UseEnterpriseServiceDefaults();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new { Service = "CatalogService", Status = "Running" }));

// REST 給外部世界用，例如 Swagger、前端或 Postman。
app.MapGet("/api/products", async (ISender sender, CancellationToken cancellationToken) =>
    Results.Ok(await sender.Send(new ListProductsQuery(), cancellationToken)));
app.MapGet("/api/products/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
{
    var product = await sender.Send(new GetProductQuery(id), cancellationToken);
    return product is null ? Results.NotFound() : Results.Ok(product);
});
app.MapPost("/api/products", async (CreateProductCommand command, ISender sender, CancellationToken cancellationToken) =>
{
    var created = await sender.Send(command, cancellationToken);
    return Results.Created($"/api/products/{created.ProductId}", created);
});

// gRPC 給內部服務用，例如 Ordering 來問商品資料。
app.MapGrpcService<CatalogGrpcService>();

app.Run();

public partial class Program;
