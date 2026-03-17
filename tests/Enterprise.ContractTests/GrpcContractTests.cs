using CatalogService.Contracts;
using FluentAssertions;
using InventoryService.Contracts;
using OrderingService.Contracts;

namespace Enterprise.ContractTests;

public sealed class GrpcContractTests
{
    [Fact]
    public void Catalog_contract_should_generate_grpc_client_and_service_base()
    {
        typeof(CatalogGrpc.CatalogGrpcClient).Should().NotBeNull();
        typeof(CatalogGrpc.CatalogGrpcBase).Should().NotBeNull();
    }

    [Fact]
    public void Inventory_contract_should_generate_grpc_client_and_service_base()
    {
        typeof(InventoryGrpc.InventoryGrpcClient).Should().NotBeNull();
        typeof(InventoryGrpc.InventoryGrpcBase).Should().NotBeNull();
    }

    [Fact]
    public void Ordering_event_contract_should_keep_v1_as_default_version()
    {
        var integrationEvent = new OrderPlacedIntegrationEvent();
        integrationEvent.Version.Should().Be("v1");
    }
}
