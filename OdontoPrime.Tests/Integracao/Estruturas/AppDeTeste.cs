using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OdontoPrime.Infra.Jobs;
using OdontoPrime.Infra.Mensageria.Outbox;
using Microsoft.EntityFrameworkCore;
using OdontoPrime.Data;

namespace OdontoPrime.Tests;

public class AppDeTeste(string postgres, string rabbitMq, RelogioDeTeste relogio)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = postgres,
                ["RabbitMq:ConnectionString"] = rabbitMq
            }));

        builder.ConfigureTestServices(services =>
        {
            // Portão das gravações: desarmado por padrão, só o teste da corrida arma.
            services.AddSingleton<PortaoDeGravacao>();
            services.ConfigureDbContext<AppDbContext>((sp, opcoes) => opcoes.AddInterceptors(sp.GetRequiredService<PortaoDeGravacao>()));
            
            
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(relogio);

            // Os jobs não rodam sozinhos no teste: o teste chama um ciclo quando quiser.
            RemoverHostedService<LembreteScheduler>(services);
            RemoverHostedService<RelayDaOutbox>(services);
            RemoverHostedService<ManutencaoDaOutbox>(services);
        });
    }

    private static void RemoverHostedService<T>(IServiceCollection services) where T : IHostedService
        => services.Remove(services.Single(d => d.ImplementationType == typeof(T)));
}