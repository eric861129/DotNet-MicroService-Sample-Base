[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Name
)

. (Join-Path $PSScriptRoot 'common.ps1')

$repoRoot = Get-RepoRoot
$domainName = Get-PascalCaseName -Value $Name
if ($domainName.EndsWith('Service', [System.StringComparison]::Ordinal)) {
    $domainName = $domainName.Substring(0, $domainName.Length - 'Service'.Length)
}

$servicePrefix = "$domainName" + 'Service'
$serviceRoot = Join-Path $repoRoot "src\Services\$domainName\$servicePrefix.Api"

if (Test-Path $serviceRoot) {
    throw "Service directory already exists: $serviceRoot"
}

$files = @{
    "$servicePrefix.Api.csproj" = @"
<Project Sdk=""Microsoft.NET.Sdk.Web"">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.AspNetCore.OpenApi"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore.Design"">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore.Tools"">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\..\BuildingBlocks\Enterprise.ServiceDefaults\Enterprise.ServiceDefaults.csproj"" />
  </ItemGroup>
</Project>
"@
    "Program.cs" = @"
using Enterprise.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using $servicePrefix.Api;
using $servicePrefix.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEnterpriseServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<$domainName`DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString(""$domainName`Db"")));

var app = builder.Build();

app.UseEnterpriseServiceDefaults();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<$domainName`DbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.MapGet(""/"", () => Results.Ok(new { Service = ""$servicePrefix"", Mode = ""BaseLite"" }));
app.MapGet(""/api/records"", async ($domainName`DbContext dbContext, CancellationToken cancellationToken) =>
{
    var records = await dbContext.Records.OrderBy(x => x.Name).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    return Results.Ok(records);
});
app.MapPost(""/api/records"", async (Create$domainName`RecordRequest request, $domainName`DbContext dbContext, CancellationToken cancellationToken) =>
{
    var validationErrors = request.Validate();
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var record = $domainName`Record.Create(request.Name, request.Description);
    dbContext.Records.Add(record);
    await dbContext.SaveChangesAsync(cancellationToken);
    return Results.Created($""/api/records/{record.Id}"", record.ToResponse());
});

app.Run();
"@
    "$servicePrefix`Marker.cs" = @"
namespace $servicePrefix.Api;

public sealed class $servicePrefix`Marker
{
}
"@
    "Create$domainName`RecordRequest.cs" = @"
namespace $servicePrefix.Api;

public sealed record Create$domainName`RecordRequest(string Name, string Description)
{
    public Dictionary<string, string[]> Validate()
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(Name))
        {
            errors[""name""] = [""Name is required.""];
        }

        return errors;
    }
}
"@
    "$domainName`Record.cs" = @"
namespace $servicePrefix.Api;

public sealed class $domainName`Record
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public static $domainName`Record Create(string name, string description)
    {
        return new $domainName`Record
        {
            Name = name.Trim(),
            Description = description.Trim()
        };
    }

    public $domainName`RecordResponse ToResponse() => new(Id, Name, Description);
}
"@
    "$domainName`RecordResponse.cs" = @"
namespace $servicePrefix.Api;

public sealed record $domainName`RecordResponse(Guid Id, string Name, string Description);
"@
    "Data\\$domainName`DbContext.cs" = @"
using Microsoft.EntityFrameworkCore;

namespace $servicePrefix.Api.Data;

public sealed class $domainName`DbContext(DbContextOptions<$domainName`DbContext> options) : DbContext(options)
{
    public DbSet<$domainName`Record> Records => Set<$domainName`Record>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<$domainName`Record>(builder =>
        {
            builder.ToTable(""$domainName`Records"");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(2000);
        });
    }
}
"@
    "appsettings.json" = @"
{
  ""ConnectionStrings"": {
    ""$domainName`Db"": ""Server=localhost,1433;Database=$domainName`Db;User Id=sa;Password=Your_password123;TrustServerCertificate=true""
  },
  ""AllowedHosts"": ""*""
}
"@
    "appsettings.Development.json" = @"
{
  ""ConnectionStrings"": {
    ""$domainName`Db"": ""Server=localhost,1433;Database=$domainName`Db;User Id=sa;Password=Your_password123;TrustServerCertificate=true""
  }
}
"@
    "Dockerfile" = @"
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish src/Services/$domainName/$servicePrefix.Api/$servicePrefix.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT [""dotnet"", ""$servicePrefix.Api.dll""]
"@
    "$servicePrefix.Api.http" = @"
@host = http://localhost:5000

### Create a record
POST {{host}}/api/records
Content-Type: application/json

{
  ""name"": ""First $domainName"",
  ""description"": ""Created by the Base Lite scaffolder.""
}

### List records
GET {{host}}/api/records
"@
    "Properties\\launchSettings.json" = @"
{
  ""$schema"": ""https://json.schemastore.org/launchsettings.json"",
  ""profiles"": {
    ""http"": {
      ""commandName"": ""Project"",
      ""dotnetRunMessages"": true,
      ""launchBrowser"": false,
      ""applicationUrl"": ""http://localhost:5000"",
      ""environmentVariables"": {
        ""ASPNETCORE_ENVIRONMENT"": ""Development""
      }
    }
  }
}
"@
}

Write-Step "Scaffolding service: $servicePrefix"

foreach ($relativePath in $files.Keys) {
    $fullPath = Join-Path $serviceRoot $relativePath
    $directory = Split-Path -Parent $fullPath
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
    Set-Content -Path $fullPath -Value $files[$relativePath] -Encoding utf8
}

Push-Location $repoRoot
try {
    & dotnet sln 'EnterpriseMicroservicesBoilerplate.sln' add (Join-Path $serviceRoot "$servicePrefix.Api.csproj") | Out-Null
}
finally {
    Pop-Location
}

Write-Step 'Next steps'
Write-Note "Add $servicePrefix.Api to docker-compose.yml if it should run in the local stack."
Write-Note 'Add a Gateway route if the service should be externally reachable.'
Write-Note 'Create integration tests before adding feature-specific logic.'
