using FluentAssertions;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace Enterprise.IntegrationTests;

public sealed class TestcontainersSmokeTests
{
    [Fact]
    public async Task MsSql_and_rabbitmq_containers_should_be_able_to_start_when_enabled()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RUN_CONTAINER_TESTS"), "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await using var sql = new MsSqlBuilder().Build();
        await using var rabbitMq = new RabbitMqBuilder().Build();

        await sql.StartAsync();
        await rabbitMq.StartAsync();

        sql.GetConnectionString().Should().Contain("Server=");
        rabbitMq.GetConnectionString().Should().Contain("amqp://");
    }
}
