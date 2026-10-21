using OdontoPrime.Infra.Mensageria.Outbox;

namespace OdontoPrime.Tests;

[Collection(ColecaoDeIntegracao.Nome)]
public class ManutencaoDaOutboxTests(InfraDeTeste infra) : TesteDeIntegracao(infra)
{
    [Fact]
    public async Task Apaga_so_as_publicadas_ha_mais_de_7_dias()
    {
        var agora = Relogio.GetUtcNow().UtcDateTime;

        var antiga = OutboxMensagem.Criar(new EventoDeTeste(1), "teste.antiga", agora.AddDays(-10));
        antiga.MarcarComoPublicada(agora.AddDays(-8));

        var recente = OutboxMensagem.Criar(new EventoDeTeste(2), "teste.recente", agora.AddDays(-2));
        recente.MarcarComoPublicada(agora.AddDays(-1));

        var pendente = OutboxMensagem.Criar(new EventoDeTeste(3), "teste.pendente", agora.AddDays(-10));

        await InserirNaOutboxAsync(antiga, recente, pendente);

        await RodarManutencaoAsync();

        var restantes = await OutboxAsync();
        Assert.Equal(new[] { "teste.pendente", "teste.recente" }, restantes.Select(m => m.RoutingKey));
    }
}