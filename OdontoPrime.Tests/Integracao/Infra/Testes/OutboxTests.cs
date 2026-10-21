using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OdontoPrime.Application;
using OdontoPrime.Data;
using OdontoPrime.Infra.Mensageria;
using OdontoPrime.Infra.Mensageria.Outbox;
using Xunit;

namespace OdontoPrime.Tests;

[Collection(ColecaoDeIntegracao.Nome)]
public class OutboxTests(InfraDeTeste infra) : TesteDeIntegracao(infra)
{
    [Fact]
    public async Task Agendar_grava_a_consulta_e_o_evento_juntos()
    {
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();

        var consultaId = await AgendarAsync(pacienteId, dentistaId, NaClinica(10, 11, 10));

        var consulta = await ConsultaAsync(consultaId);
        var mensagem = Assert.Single(await OutboxAsync());
        Assert.Equal("ConsultaAgendada", mensagem.Tipo);
        Assert.Equal(RoutingKeys.ConsultaAgendada, mensagem.RoutingKey);
        Assert.Null(mensagem.PublicadoEm);

        // O formato que o Java espera: camelCase e datas com o offset de Salvador.
        using var json = JsonDocument.Parse(mensagem.Payload);
        var evento = json.RootElement;
        Assert.Equal(consulta.Id, evento.GetProperty("consultaId").GetGuid());
        Assert.Equal("2026-10-11T10:00:00-03:00", evento.GetProperty("dataHora").GetString());
        Assert.Equal("2026-10-09T10:00:00-03:00", evento.GetProperty("ocorridoEm").GetString());
    }

    [Fact]
    public async Task Relay_publica_no_RabbitMQ_e_marca_como_publicada()
    {
        await using var fila = await FilaDeTeste.CriarAsync(Infra.RabbitMqConnectionString, "consulta.*", "lembrete.*");
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();
        await AgendarAsync(pacienteId, dentistaId, NaClinica(10, 11, 10));

        await RodarRelayAsync();

        var mensagem = Assert.Single(await OutboxAsync());
        Assert.NotNull(mensagem.PublicadoEm);

        var recebida = await fila.PegarAsync();
        Assert.NotNull(recebida);
        Assert.Equal(mensagem.Id.ToString(), recebida.BasicProperties.MessageId);
        Assert.Equal("ConsultaAgendada", recebida.BasicProperties.Type);
        Assert.Equal(RoutingKeys.ConsultaAgendada, recebida.RoutingKey);
        Assert.Equal(mensagem.Payload, Encoding.UTF8.GetString(recebida.Body.Span));
    }

    [Fact]
    public async Task Relay_para_o_lote_na_primeira_falha_para_manter_a_ordem()
    {
        await using var fila = await FilaDeTeste.CriarAsync(Infra.RabbitMqConnectionString, "consulta.*", "lembrete.*");
        var agora = Relogio.GetUtcNow().UtcDateTime;

        // A primeira vai para uma routing key que nenhuma fila escuta; a segunda, para uma que a fila escuta.
        await InserirNaOutboxAsync(
            OutboxMensagem.Criar(new EventoDeTeste(1), "teste.sem-destino", agora),
            OutboxMensagem.Criar(new EventoDeTeste(2), RoutingKeys.ConsultaAgendada, agora.AddSeconds(1)));

        await RodarRelayAsync();

        var mensagens = await OutboxAsync();
        Assert.Equal(2, mensagens.Count);
        var (semDestino, seguinte) = (mensagens[0], mensagens[1]);

        Assert.Null(semDestino.PublicadoEm);
        Assert.Equal(1, semDestino.Tentativas);
        Assert.NotNull(semDestino.UltimoErro);

        Assert.Null(seguinte.PublicadoEm);      // nem tentou: esperou atrás da primeira
        Assert.Equal(0, seguinte.Tentativas);
        Assert.Null(await fila.PegarAsync());   // nada chegou no RabbitMQ
    }
}