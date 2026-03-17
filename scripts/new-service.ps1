[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Name,
    [switch] $SkipSolution,
    [switch] $DryRun
)

. (Join-Path $PSScriptRoot 'common.ps1')

$repoRoot = Get-RepoRoot
$domainName = Get-PascalCaseName -Value $Name
if ($domainName.EndsWith('Service', [System.StringComparison]::Ordinal)) {
    $domainName = $domainName.Substring(0, $domainName.Length - 'Service'.Length)
}

$servicePrefix = "$domainName" + 'Service'
$serviceKebab = Get-KebabCaseName -Value $domainName
$serviceRoot = Join-Path $repoRoot "src\Services\$domainName"

$projectNames = @{
    Domain = "$servicePrefix.Domain"
    Application = "$servicePrefix.Application"
    Infrastructure = "$servicePrefix.Infrastructure"
    Api = "$servicePrefix.Api"
    Contracts = "$servicePrefix.Contracts"
}

if (Test-Path $serviceRoot) {
    throw "Service directory already exists: $serviceRoot"
}

$files = @{
    "$($projectNames.Domain)\$($projectNames.Domain).csproj" = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\BuildingBlocks\Enterprise.SharedKernel\Enterprise.SharedKernel.csproj" />
  </ItemGroup>
</Project>
"@
    "$($projectNames.Domain)\$domainName`Record.cs" = @"
using Enterprise.SharedKernel.Domain;

namespace $($projectNames.Domain);

public sealed class $domainName`Record : AggregateRoot
{
    private $domainName`Record()
    {
    }

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
}
"@
    "$($projectNames.Application)\$($projectNames.Application).csproj" = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\BuildingBlocks\Enterprise.Application\Enterprise.Application.csproj" />
    <ProjectReference Include="..\$($projectNames.Domain)\$($projectNames.Domain).csproj" />
  </ItemGroup>
</Project>
"@
    "$($projectNames.Application)\$domainName`RecordDto.cs" = @"
namespace $($projectNames.Application);

public sealed record $domainName`RecordDto(Guid Id, string Name, string Description);
"@
    "$($projectNames.Application)\I$domainName`Repository.cs" = @"
namespace $($projectNames.Application);

public interface I$domainName`Repository
{
    Task AddAsync($($projectNames.Domain).$domainName`Record record, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<$($projectNames.Domain).$domainName`Record>> ListAsync(CancellationToken cancellationToken = default);
}
"@
    "$($projectNames.Application)\List$domainName`RecordsQuery.cs" = @"
using Enterprise.Application.Abstractions;
using MediatR;

namespace $($projectNames.Application);

public sealed record List$domainName`RecordsQuery() : IQuery<IReadOnlyCollection<$domainName`RecordDto>>;

public sealed class List$domainName`RecordsQueryHandler(I$domainName`Repository repository)
    : IRequestHandler<List$domainName`RecordsQuery, IReadOnlyCollection<$domainName`RecordDto>>
{
    public async Task<IReadOnlyCollection<$domainName`RecordDto>> Handle(List$domainName`RecordsQuery request, CancellationToken cancellationToken)
    {
        var records = await repository.ListAsync(cancellationToken);
        return records.Select(x => new $domainName`RecordDto(x.Id, x.Name, x.Description)).ToArray();
    }
}
"@
    "$($projectNames.Application)\Create$domainName`RecordCommand.cs" = @"
using Enterprise.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace $($projectNames.Application);

public sealed record Create$domainName`RecordCommand(string Name, string Description) : ICommand<$domainName`RecordDto>;

public sealed class Create$domainName`RecordCommandValidator : AbstractValidator<Create$domainName`RecordCommand>
{
    public Create$domainName`RecordCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public sealed class Create$domainName`RecordCommandHandler(I$domainName`Repository repository)
    : IRequestHandler<Create$domainName`RecordCommand, $domainName`RecordDto>
{
    public async Task<$domainName`RecordDto> Handle(Create$domainName`RecordCommand request, CancellationToken cancellationToken)
    {
        var record = $($projectNames.Domain).$domainName`Record.Create(request.Name, request.Description);
        await repository.AddAsync(record, cancellationToken);
        return new $domainName`RecordDto(record.Id, record.Name, record.Description);
    }
}
"@
    "$($projectNames.Contracts)\$($projectNames.Contracts).csproj" = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Google.Protobuf" />
    <PackageReference Include="Grpc.Net.Client" />
    <PackageReference Include="Grpc.Tools">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <Protobuf Include="Protos\$($domainName.ToLowerInvariant()).proto" GrpcServices="Both" />
  </ItemGroup>
</Project>
"@
    "$($projectNames.Contracts)\Protos\$($domainName.ToLowerInvariant()).proto" = @"
syntax = "proto3";

option csharp_namespace = "$($projectNames.Contracts)";

package $($domainName.ToLowerInvariant());

service $servicePrefix`Grpc {
  rpc GetServiceInfo (GetServiceInfoRequest) returns (GetServiceInfoReply);
}

message GetServiceInfoRequest {
}

message GetServiceInfoReply {
  string service_name = 1;
  string status = 2;
}
"@
    "$($projectNames.Infrastructure)\$($projectNames.Infrastructure).csproj" = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MassTransit" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\BuildingBlocks\Enterprise.Persistence\Enterprise.Persistence.csproj" />
    <ProjectReference Include="..\$($projectNames.Application)\$($projectNames.Application).csproj" />
    <ProjectReference Include="..\$($projectNames.Contracts)\$($projectNames.Contracts).csproj" />
    <ProjectReference Include="..\$($projectNames.Domain)\$($projectNames.Domain).csproj" />
  </ItemGroup>
</Project>
"@
    "$($projectNames.Infrastructure)\$domainName`DbContext.cs" = @"
using Enterprise.Persistence;
using Microsoft.EntityFrameworkCore;

namespace $($projectNames.Infrastructure);

public sealed class $domainName`DbContext(DbContextOptions<$domainName`DbContext> options)
    : ServiceDbContext(options)
{
    public DbSet<$($projectNames.Domain).$domainName`Record> Records => Set<$($projectNames.Domain).$domainName`Record>();

    protected override void ConfigureDomain(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<$($projectNames.Domain).$domainName`Record>(builder =>
        {
            builder.ToTable("$domainName`Records");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(2000);
        });
    }
}
"@
    "$($projectNames.Infrastructure)\$domainName`Repository.cs" = @"
using Microsoft.EntityFrameworkCore;

namespace $($projectNames.Infrastructure);

public sealed class $domainName`Repository($domainName`DbContext dbContext) : $($projectNames.Application).I$domainName`Repository
{
    public async Task AddAsync($($projectNames.Domain).$domainName`Record record, CancellationToken cancellationToken = default)
    {
        dbContext.Records.Add(record);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<$($projectNames.Domain).$domainName`Record>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Records
            .OrderBy(x => x.Name)
            .ToArrayAsync(cancellationToken);
    }
}
"@
    "$($projectNames.Infrastructure)\$servicePrefix`GrpcService.cs" = @"
using Grpc.Core;

namespace $($projectNames.Infrastructure);

public sealed class $servicePrefix`GrpcService : $($projectNames.Contracts).$servicePrefix`Grpc.$servicePrefix`GrpcBase
{
    public override Task<$($projectNames.Contracts).GetServiceInfoReply> GetServiceInfo($($projectNames.Contracts).GetServiceInfoRequest request, ServerCallContext context)
    {
        return Task.FromResult(new $($projectNames.Contracts).GetServiceInfoReply
        {
            ServiceName = "$servicePrefix",
            Status = "Running"
        });
    }
}
"@
    "$($projectNames.Infrastructure)\DependencyInjection.cs" = @"
using Enterprise.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace $($projectNames.Infrastructure);

public static class DependencyInjection
{
    public static IServiceCollection Add$domainName`Infrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<$domainName`DbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("$domainName`Db")));

        services.AddScoped<$($projectNames.Application).I$domainName`Repository, $domainName`Repository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<$domainName`DbContext>());

        return services;
    }
}
"@
    "$($projectNames.Api)\$($projectNames.Api).csproj" = @"
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Grpc.AspNetCore" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\BuildingBlocks\Enterprise.Configuration\Enterprise.Configuration.csproj" />
    <ProjectReference Include="..\..\..\BuildingBlocks\Enterprise.Observability\Enterprise.Observability.csproj" />
    <ProjectReference Include="..\..\..\BuildingBlocks\Enterprise.Security\Enterprise.Security.csproj" />
    <ProjectReference Include="..\..\..\BuildingBlocks\Enterprise.ServiceDefaults\Enterprise.ServiceDefaults.csproj" />
    <ProjectReference Include="..\$($projectNames.Application)\$($projectNames.Application).csproj" />
    <ProjectReference Include="..\$($projectNames.Contracts)\$($projectNames.Contracts).csproj" />
    <ProjectReference Include="..\$($projectNames.Infrastructure)\$($projectNames.Infrastructure).csproj" />
  </ItemGroup>
</Project>
"@
    "$($projectNames.Api)\Program.cs" = @"
using Enterprise.Application;
using Enterprise.Configuration;
using Enterprise.Observability;
using Enterprise.Persistence;
using Enterprise.Security;
using Enterprise.ServiceDefaults;
using MediatR;
using $($projectNames.Application);
using $($projectNames.Infrastructure);

var builder = WebApplication.CreateBuilder(args);

builder.AddEnterpriseConfiguration();
builder.AddEnterpriseObservability("$servicePrefix");

builder.Services.AddEnterpriseServiceDefaults(builder.Configuration);
builder.Services.AddEnterpriseJwtAuthentication(builder.Configuration);
builder.Services.AddEnterpriseApplication(typeof(Create$domainName`RecordCommand).Assembly);
builder.Services.Add$domainName`Infrastructure(builder.Configuration);
builder.Services.AddGrpc();
builder.Services.AddOpenApi();
builder.Services.AddHostedService<DatabaseMigrationHostedService<$domainName`DbContext>>();

var app = builder.Build();

app.UseEnterpriseServiceDefaults();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new { Service = "$servicePrefix", Status = "Running" }));
app.MapGet("/api/records", async (ISender sender, CancellationToken cancellationToken) =>
    Results.Ok(await sender.Send(new List$domainName`RecordsQuery(), cancellationToken)));
app.MapPost("/api/records", async (Create$domainName`RecordCommand command, ISender sender, CancellationToken cancellationToken) =>
{
    var created = await sender.Send(command, cancellationToken);
    return Results.Created($"/api/records/{created.Id}", created);
});
app.MapGrpcService<$servicePrefix`GrpcService>();

app.Run();

public partial class Program;
"@
    "$($projectNames.Api)\appsettings.json" = @"
{
  "ConnectionStrings": {
    "$domainName`Db": "Server=localhost,1433;Database=$domainName`Db;User Id=sa;Password=ChangeMe123!;TrustServerCertificate=true"
  },
  "AllowedHosts": "*"
}
"@
    "$($projectNames.Api)\appsettings.Development.json" = @"
{
  "ConnectionStrings": {
    "$domainName`Db": "Server=localhost,1433;Database=$domainName`Db;User Id=sa;Password=ChangeMe123!;TrustServerCertificate=true"
  }
}
"@
    "$($projectNames.Api)\Dockerfile" = @"
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish src/Services/$domainName/$($projectNames.Api)/$($projectNames.Api).csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "$($projectNames.Api).dll"]
"@
    "$($projectNames.Api)\$($projectNames.Api).http" = @"
@host = http://localhost:5000

### Create a record
POST {{host}}/api/records
Content-Type: application/json

{
  "name": "My First $domainName",
  "description": "Generated by the service scaffolder."
}

### List records
GET {{host}}/api/records
"@
}

Write-Step "Scaffolding service: $servicePrefix"

foreach ($relativePath in $files.Keys) {
    $fullPath = Join-Path $serviceRoot $relativePath
    $directory = Split-Path -Parent $fullPath

    if ($DryRun) {
        Write-Note "Would create: $fullPath"
        continue
    }

    New-Item -ItemType Directory -Path $directory -Force | Out-Null
    Set-Content -Path $fullPath -Value $files[$relativePath] -Encoding utf8
}

if (-not $DryRun -and -not $SkipSolution) {
    Write-Step 'Adding projects to the solution'
    Push-Location $repoRoot
    try {
        & dotnet sln 'EnterpriseMicroservicesBoilerplate.sln' add `
            (Join-Path $serviceRoot "$($projectNames.Domain)\$($projectNames.Domain).csproj") `
            (Join-Path $serviceRoot "$($projectNames.Application)\$($projectNames.Application).csproj") `
            (Join-Path $serviceRoot "$($projectNames.Infrastructure)\$($projectNames.Infrastructure).csproj") `
            (Join-Path $serviceRoot "$($projectNames.Api)\$($projectNames.Api).csproj") `
            (Join-Path $serviceRoot "$($projectNames.Contracts)\$($projectNames.Contracts).csproj") | Out-Null
    }
    finally {
        Pop-Location
    }
}

Write-Step 'Next steps'
Write-Note "Add $serviceKebab-service-api and a database container to docker-compose.yml."
Write-Note 'Add a Gateway route and cluster if the service should be externally reachable.'
Write-Note 'Run scripts/new-event.ps1 if this service needs integration events.'
Write-Note 'Create the first EF migration with dotnet ef migrations add.'
