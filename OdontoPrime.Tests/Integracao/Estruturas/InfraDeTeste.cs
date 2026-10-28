using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OdontoPrime.Data;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace OdontoPrime.Tests;

public class InfraDeTeste : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder("rabbitmq:4-alpine").Build();
    
    
    public RelogioDeTeste Relogio { get; } = new();
    public AppDeTeste App { get; private set; } = null!;
    public string RabbitMqConnectionString => _rabbitMq.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _rabbitMq.StartAsync());

        App = new AppDeTeste(_postgres.GetConnectionString(), _rabbitMq.GetConnectionString(), Relogio);

        using var escopo = App.Services.CreateScope();
        await escopo.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (App is not null) await App.DisposeAsync();
        await _postgres.DisposeAsync();
        await _rabbitMq.DisposeAsync();
    }
}

[CollectionDefinition(Nome)]
public class ColecaoDeIntegracao : ICollectionFixture<InfraDeTeste>
{
    public const string Nome = "Integracao";
}