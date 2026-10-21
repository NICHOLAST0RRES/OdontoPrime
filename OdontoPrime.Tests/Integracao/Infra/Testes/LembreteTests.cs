using System.Text.Json;
using Clinica.Contratos;
using Microsoft.Extensions.DependencyInjection;
using OdontoPrime.Application;
using OdontoPrime.Infra.Mensageria.Outbox;

namespace OdontoPrime.Tests;

[Collection(ColecaoDeIntegracao.Nome)]
public class LembreteTests(InfraDeTeste infra) : TesteDeIntegracao(infra)
{
    [Fact]
    public async Task Lembrete_sai_uma_unica_vez_na_vespera_as_18h()
    {
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();
        var consultaId = await AgendarAsync(pacienteId, dentistaId, NaClinica(10, 11, 10)); // domingo 10h

        Assert.Equal(NaClinica(10, 10, 18).UtcDateTime, (await ConsultaAsync(consultaId)).LembrarEm);

        // Sábado 17h59: ainda não.
        Relogio.Ajustar(NaClinica(10, 10, 17, 59));  // testa a primeira vez fora do horario
        await RodarSchedulerAsync();
        Assert.Empty(await LembretesAsync());

        // Sábado 18h01: sai.
        Relogio.Ajustar(NaClinica(10, 10, 18, 1));  // testa agora depois do horario
        await RodarSchedulerAsync();
        Assert.Single(await LembretesAsync());
        Assert.Equal(NaClinica(10, 10, 18, 1).UtcDateTime, (await ConsultaAsync(consultaId)).LembreteEnviadoEm);

        // Um minuto depois o scheduler roda de novo: não pode duplicar.
        Relogio.Ajustar(NaClinica(10, 10, 18, 2));
        await RodarSchedulerAsync();
        Assert.Single(await LembretesAsync());
    }

    [Fact]
    public async Task Reagendar_recalcula_o_lembrete_e_libera_um_novo_envio()
    {
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();
        var consultaId = await AgendarAsync(pacienteId, dentistaId, NaClinica(10, 11, 10)); // domingo 10h

        Relogio.Ajustar(NaClinica(10, 10, 18, 1));
        await RodarSchedulerAsync();                                                       // 1º lembrete

        Relogio.Ajustar(NaClinica(10, 10, 18, 5));
        await ReagendarAsync(consultaId, NaClinica(10, 13, 9));                             // terça 9h

        var consulta = await ConsultaAsync(consultaId);
        Assert.Equal(NaClinica(10, 12, 18).UtcDateTime, consulta.LembrarEm);                // segunda 18h
        Assert.Null(consulta.LembreteEnviadoEm);

        Relogio.Ajustar(NaClinica(10, 12, 18, 1));
        await RodarSchedulerAsync();                                                       // 2º lembrete

        var lembretes = await LembretesAsync();
        Assert.Equal(2, lembretes.Count);

        using var json = JsonDocument.Parse(lembretes[1].Payload);
        Assert.Equal("2026-10-13T09:00:00-03:00", json.RootElement.GetProperty("dataHora").GetString());
    }

    [Fact]
    public async Task Consulta_cancelada_nao_recebe_lembrete()
    {
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();
        var consultaId = await AgendarAsync(pacienteId, dentistaId, NaClinica(10, 11, 10));

        await CancelarAsync(consultaId);
        Assert.Null((await ConsultaAsync(consultaId)).LembrarEm);

        Relogio.Ajustar(NaClinica(10, 10, 18, 1));
        await RodarSchedulerAsync();

        Assert.Empty(await LembretesAsync());
    }

    private async Task<List<OutboxMensagem>> LembretesAsync()
        => (await OutboxAsync()).Where(m => m.Tipo == nameof(LembreteDeConsulta)).ToList();

    private async Task ReagendarAsync(Guid consultaId, DateTimeOffset novaDataHora)
    {
        using var escopo = NovoEscopo();
        var resultado = await escopo.ServiceProvider.GetRequiredService<ConsultaService>()
            .ReagendarAsync(consultaId, novaDataHora.UtcDateTime);
        Assert.True(resultado.Sucesso, resultado.Erro);
    }

    private async Task CancelarAsync(Guid consultaId)
    {
        using var escopo = NovoEscopo();
        var resultado = await escopo.ServiceProvider.GetRequiredService<ConsultaService>()
            .CancelarAsync(consultaId);
        Assert.True(resultado.Sucesso, resultado.Erro);
    }
}