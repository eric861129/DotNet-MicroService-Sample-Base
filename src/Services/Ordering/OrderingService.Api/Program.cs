using Enterprise.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using OrderingService.Api;
using OrderingService.Api.Catalog;
using OrderingService.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEnterpriseServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<OrderingDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("OrderingDb");
    if (!string.IsNullOrWhiteSpace(connectionString) && connectionString.StartsWith("InMemory:", StringComparison.Ordinal))
    {
        options.UseInMemoryDatabase(connectionString["InMemory:".Length..]);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});
builder.Services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Catalog:BaseUrl"] ?? "http://localhost:5191/");
});

var app = builder.Build();

app.UseEnterpriseServiceDefaults();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.MapGet("/", () => Results.Ok(new { Service = "OrderingService", Mode = "BaseLite" }));

app.MapGet("/api/orders/{id:guid}", async (Guid id, OrderingDbContext dbContext, CancellationToken cancellationToken) =>
{
    var order = await dbContext.Orders
        .Include(candidate => candidate.Items)
        .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

    return order is null ? Results.NotFound() : Results.Ok(order.ToResponse());
});

app.MapPost("/api/orders", async (CreateOrderRequest request, OrderingDbContext dbContext, ICatalogServiceClient catalogServiceClient, CancellationToken cancellationToken) =>
{
    var validationErrors = request.Validate();
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var items = new List<OrderItem>();

    foreach (var requestedItem in request.Items)
    {
        CatalogProductResponse? product;
        try
        {
            product = await catalogServiceClient.GetProductAsync(requestedItem.ProductId, cancellationToken);
        }
        catch (CatalogServiceUnavailableException exception)
        {
            return Results.Problem(title: "Catalog service unavailable", detail: exception.Message, statusCode: StatusCodes.Status502BadGateway);
        }

        if (product is null)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [$"items[{items.Count}].productId"] = [$"Product {requestedItem.ProductId} was not found."]
            });
        }

        items.Add(OrderItem.Create(product.ProductId, product.Sku, product.Name, product.Price, requestedItem.Quantity));
    }

    var order = Order.Create(request.CustomerEmail, items);
    dbContext.Orders.Add(order);
    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/orders/{order.Id}", order.ToResponse());
});

app.Run();
