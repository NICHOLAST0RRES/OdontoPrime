using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OdontoPrime.Application;
using OdontoPrime.Data;
using OdontoPrime.Domain.Models;
using OdontoPrime.Infra.Jobs;
using OdontoPrime.Infra.Mensageria.Outbox;
using Xunit;

namespace OdontoPrime.Tests;

// Base de todos os testes de integração: banco limpo e relógio conhecido antes de cada teste.
public abstract class TesteDeIntegracao(InfraDeTeste infra) : IAsyncLifetime
{
    protected InfraDeTeste Infra { get; } = infra;
    protected RelogioDeTeste Relogio => Infra.Relogio;
    protected static CancellationToken Ct => TestContext.Current.CancellationToken;

    // Para testes que só precisam de "uma mensagem qualquer" na outbox.
    protected record EventoDeTeste(int Numero);

    public async ValueTask InitializeAsync()
    {
        using var escopo = NovoEscopo();
        var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.ExecuteSqlRawAsync(
            """TRUNCATE "OutboxMensagens", "Consultas", "Pacientes", "Profissionais", "Convenios" RESTART IDENTITY CASCADE""",
            Ct);

        Relogio.Ajustar(NaClinica(10, 9, 10)); // padrão: sexta, 09/10, 10h em Salvador
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    protected IServiceScope NovoEscopo() => Infra.App.Services.CreateScope();

    protected static DateTimeOffset NaClinica(int mes, int dia, int hora, int minuto = 0)
        => new(2026, mes, dia, hora, minuto, 0, TimeSpan.FromHours(-3));

    // ---------- Preparar ----------

    protected async Task<(Guid PacienteId, Guid DentistaId)> CriarPacienteEDentistaAsync()
    {
        using var escopo = NovoEscopo();
        var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();

        var convenio = new Convenio { Nome = "Particular" };
        db.Convenios.Add(convenio);
        await db.SaveChangesAsync(Ct);

        var paciente = new Paciente("Maria Teste", "52998224725", "71999990000", "maria@teste.com", convenio.Id);
        var dentista = new Profissional("Dr. Teste", "71988880000", TipoProfissional.DentistaId, "CRO-BA 1234");
        db.AddRange(paciente, dentista);
        await db.SaveChangesAsync(Ct);

        return (paciente.Id, dentista.Id);
    }

    protected async Task<Guid> AgendarAsync(Guid pacienteId, Guid dentistaId, DateTimeOffset dataHora)
    {
        using var escopo = NovoEscopo();
        var servico = escopo.ServiceProvider.GetRequiredService<ConsultaService>();

        var resultado = await servico.AgendarAsync(pacienteId, dentistaId, dataHora.UtcDateTime, observacao: null);

        Assert.True(resultado.Sucesso, resultado.Erro);
        return resultado.Valor!.Id;
    }

    protected async Task InserirNaOutboxAsync(params OutboxMensagem[] mensagens)
    {
        using var escopo = NovoEscopo();
        var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();

        db.OutboxMensagens.AddRange(mensagens);
        await db.SaveChangesAsync(Ct);
    }

    // ---------- Agir: um ciclo de cada job ----------

    protected async Task RodarRelayAsync()
    {
        using var relay = ActivatorUtilities.CreateInstance<RelayDaOutbox>(Infra.App.Services);
        try
        {
            await relay.PublicarPendentesAsync(Ct);
        }
        finally
        {
            await relay.StopAsync(CancellationToken.None); // fecha a conexão com o RabbitMQ
        }
    }

    protected async Task RodarSchedulerAsync()
    {
        using var scheduler = ActivatorUtilities.CreateInstance<LembreteScheduler>(Infra.App.Services);
        await scheduler.GerarLembretesAsync(Ct);
    }

    protected async Task RodarManutencaoAsync()
    {
        using var manutencao = ActivatorUtilities.CreateInstance<ManutencaoDaOutbox>(Infra.App.Services);
        await manutencao.ExecutarCicloAsync(Ct);
    }

    // ---------- Verificar: leitura direto do banco, num escopo novo ----------

    protected async Task<List<OutboxMensagem>> OutboxAsync()
    {
        using var escopo = NovoEscopo();
        var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.OutboxMensagens.AsNoTracking()
            .OrderBy(m => m.CriadoEm).ThenBy(m => m.Id)
            .ToListAsync(Ct);
    }


    protected async Task<Consulta> ConsultaAsync(Guid id)
    {
        using var escopo = NovoEscopo();
        var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Consultas.AsNoTracking().SingleAsync(c => c.Id == id, Ct);
    }


}