using CatalogService.Api;
using CatalogService.Api.Data;
using Enterprise.ServiceDefaults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEnterpriseServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<CatalogDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("CatalogDb");
    if (!string.IsNullOrWhiteSpace(connectionString) && connectionString.StartsWith("InMemory:", StringComparison.Ordinal))
    {
        options.UseInMemoryDatabase(connectionString["InMemory:".Length..]);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

var app = builder.Build();

app.UseEnterpriseServiceDefaults();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.MapGet("/", () => Results.Ok(new { Service = "CatalogService", Mode = "BaseLite" }));

app.MapGet("/api/products", async (CatalogDbContext dbContext, CancellationToken cancellationToken) =>
{
    var products = await dbContext.Products
        .OrderBy(product => product.Name)
        .Select(product => product.ToResponse())
        .ToListAsync(cancellationToken);

    return Results.Ok(products);
});

app.MapGet("/api/products/{id:guid}", async (Guid id, CatalogDbContext dbContext, CancellationToken cancellationToken) =>
{
    var product = await dbContext.Products
        .Where(candidate => candidate.Id == id)
        .Select(candidate => candidate.ToResponse())
        .SingleOrDefaultAsync(cancellationToken);

    return product is null ? Results.NotFound() : Results.Ok(product);
});

app.MapPost("/api/products", async (CreateProductRequest request, CatalogDbContext dbContext, CancellationToken cancellationToken) =>
{
    var validationErrors = request.Validate();
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var product = Product.Create(request.Sku, request.Name, request.Price);
    dbContext.Products.Add(product);
    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/products/{product.Id}", product.ToResponse());
});

app.Run();
