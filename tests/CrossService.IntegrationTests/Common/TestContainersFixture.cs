using Testcontainers.PostgreSql;
using Testcontainers.MongoDb;
using Testcontainers.RabbitMq;

namespace CrossService.IntegrationTests.Common;

public class TestContainersFixture : IAsyncLifetime
{
    public PostgreSqlContainer PostgresCadastros { get; private set; } = null!;
    public PostgreSqlContainer PostgresOrdens { get; private set; } = null!;
    public PostgreSqlContainer PostgresEstoque { get; private set; } = null!;
    public MongoDbContainer MongoEstoque { get; private set; } = null!;
    public RabbitMqContainer RabbitMq { get; private set; } = null!;

    public string CadastrosConnectionString => PostgresCadastros.GetConnectionString();
    public string OrdensConnectionString => PostgresOrdens.GetConnectionString();
    public string EstoqueConnectionString => PostgresEstoque.GetConnectionString();
    public string MongoConnectionString => MongoEstoque.GetConnectionString();
    public string RabbitMqHost => RabbitMq.Hostname;
    public int RabbitMqPort => RabbitMq.GetMappedPublicPort(5672);

    public async Task InitializeAsync()
    {
        PostgresCadastros = new PostgreSqlBuilder()
            .WithDatabase("cadastros_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        PostgresOrdens = new PostgreSqlBuilder()
            .WithDatabase("ordens_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        PostgresEstoque = new PostgreSqlBuilder()
            .WithDatabase("estoque_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        MongoEstoque = new MongoDbBuilder()
            .Build();

        RabbitMq = new RabbitMqBuilder()
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();

        await Task.WhenAll(
            PostgresCadastros.StartAsync(),
            PostgresOrdens.StartAsync(),
            PostgresEstoque.StartAsync(),
            MongoEstoque.StartAsync(),
            RabbitMq.StartAsync());
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            PostgresCadastros.DisposeAsync().AsTask(),
            PostgresOrdens.DisposeAsync().AsTask(),
            PostgresEstoque.DisposeAsync().AsTask(),
            MongoEstoque.DisposeAsync().AsTask(),
            RabbitMq.DisposeAsync().AsTask());
    }
}

[CollectionDefinition("IntegrationTests")]
public class IntegrationTestsCollection : ICollectionFixture<TestContainersFixture>
{
}
