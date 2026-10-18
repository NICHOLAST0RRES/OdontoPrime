using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OdontoPrime.Application;
using OdontoPrime.Data;
using OdontoPrime.Infra.Mensageria;
using Xunit;

namespace OdontoPrime.Tests;

[Collection(ColecaoDeIntegracao.Nome)]

public class OutboxTests(InfraDeTeste infra) : TesteDeIntegracao(infra)
{
    [Fact]
    public async Task Agendar_grava_a_consulta_e_o_evento_juntos()
    {
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();

        using (var escopo = NovoEscopo())
        {
            var servico = escopo.ServiceProvider.GetRequiredService<ConsultaService>();
            var resultado = await servico.AgendarAsync(
                pacienteId, dentistaId, NaClinica(10, 11, 10).UtcDateTime, observacao: null);

            Assert.True(resultado.Sucesso, resultado.Erro);
        }

        using var leitura = NovoEscopo();
        var db = leitura.ServiceProvider.GetRequiredService<AppDbContext>();

        var consulta = await db.Consultas.SingleAsync();
        var mensagem = await db.OutboxMensagens.SingleAsync();

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
}