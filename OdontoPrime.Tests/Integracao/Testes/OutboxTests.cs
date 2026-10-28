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

// Testes da outbox: o agendamento grava o evento, e o relay leva esse evento até o RabbitMQ.
[Collection(ColecaoDeIntegracao.Nome)]
public class OutboxTests(InfraDeTeste infra) : TesteDeIntegracao(infra)
{
    
    [Fact]
    public async Task Agendar_grava_a_consulta_e_o_evento_juntos()
    {
        // Preparar
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();

        // Agir: agenda pra domingo 11/10 às 10h (o relógio está em sexta 09/10 às 10h)
        // 1. O AgendarAsync já confere que o service devolveu sucesso
        var consultaId = await AgendarAsync(pacienteId, dentistaId, NaClinica(10, 11, 10));

        // Verificar
        var consulta = await ConsultaAsync(consultaId);

        // 2. O Assert.Single falha com zero mensagens (evento esquecido) ou com mais de uma (duplicado)
        var mensagem = Assert.Single(await OutboxAsync());

        Assert.Equal("ConsultaAgendada", mensagem.Tipo);                    // 3.
        Assert.Equal(RoutingKeys.ConsultaAgendada, mensagem.RoutingKey);    // 4.
        Assert.Null(mensagem.PublicadoEm);                                  // 5. ainda na bandeja: quem publica é o relay

        // 6 a 8. O JSON no formato que o consumidor Java lê
        using var json = JsonDocument.Parse(mensagem.Payload);
        var evento = json.RootElement;
        Assert.Equal(consulta.Id, evento.GetProperty("consultaId").GetGuid());                      // 6.
        Assert.Equal("2026-10-11T10:00:00-03:00", evento.GetProperty("dataHora").GetString());     // 7.
        Assert.Equal("2026-10-09T10:00:00-03:00", evento.GetProperty("ocorridoEm").GetString());   // 8.
    }

    // O que ele verifica?
    // 1. Que, depois de agendar uma consulta, o relay roda e publica a mensagem
    // 2. Que a mensagem na outbox foi marcada como publicada (PublicadoEm preenchido)
    // 3. Que a mensagem chegou no RabbitMQ (a FilaDeTeste recebeu)
    // 4. Que o MessageId recebido é igual ao Id da mensagem na outbox
    // 5. Que o tipo recebido é ConsultaAgendada
    // 6. Que a routing key recebida é consulta.agendada
    // 7. Que o conteúdo recebido é exatamente o JSON que estava gravado na outbox
    [Fact]
    public async Task Relay_publica_no_RabbitMQ_e_marca_como_publicada()
    {
        // Preparar: uma fila temporária escutando os eventos de consulta e de lembrete.
        // Sem ela, o RabbitMQ devolveria a mensagem (mandatory: true) e a publicação falharia.
        await using var fila = await FilaDeTeste.CriarAsync(Infra.RabbitMqConnectionString, "consulta.*", "lembrete.*");
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();
        await AgendarAsync(pacienteId, dentistaId, NaClinica(10, 11, 10));

        // Agir
        await RodarRelayAsync();                                            // 1. um ciclo do relay

        // Verificar o lado da outbox
        var mensagem = Assert.Single(await OutboxAsync());
        Assert.NotNull(mensagem.PublicadoEm);                               // 2.

        // Verificar o lado do RabbitMQ.
        // Não precisa esperar: com a confirmação do broker, o relay só volta depois que a mensagem está na fila.
        var recebida = await fila.PegarAsync();
        Assert.NotNull(recebida);                                                         // 3.
        Assert.Equal(mensagem.Id.ToString(), recebida.BasicProperties.MessageId);         // 4. é o que o Java usa pra descartar repetidas
        Assert.Equal("ConsultaAgendada", recebida.BasicProperties.Type);                  // 5.
        Assert.Equal(RoutingKeys.ConsultaAgendada, recebida.RoutingKey);                  // 6.
        Assert.Equal(mensagem.Payload, Encoding.UTF8.GetString(recebida.Body.Span));      // 7.
    }

    // O que ele verifica?
    // 1. Que, com duas mensagens na outbox, a primeira sem fila de destino e a segunda com destino, o relay roda e a primeira falha
    // 2. Que as duas continuam na outbox
    // 3. Que a primeira não foi publicada, teve 1 tentativa registrada e guardou o erro (UltimoErro)
    // 4. Que a segunda não foi publicada e teve 0 tentativas, ou seja, nem foi tentada, porque esperou atrás da primeira
    // 5. Verifica que nada chegou no RabbitMQ (a FilaDeTeste está vazia)
    [Fact]
    public async Task Relay_para_o_lote_na_primeira_falha_para_manter_a_ordem()
    {
        // Preparar
        await using var fila = await FilaDeTeste.CriarAsync(Infra.RabbitMqConnectionString, "consulta.*", "lembrete.*");
        var agora = Relogio.GetUtcNow().UtcDateTime;

        // Duas mensagens gravadas direto na outbox, nesta ordem:
        // - a primeira vai pra uma routing key que nenhuma fila escuta;
        // - a segunda vai pra consulta.agendada, que a FilaDeTeste escuta. Ou seja, ela TERIA destino:
        //   se não chegar, é só porque o relay parou o lote.
        await InserirNaOutboxAsync(
            OutboxMensagem.Criar(new EventoDeTeste(1), "teste.sem-destino", agora),
            OutboxMensagem.Criar(new EventoDeTeste(2), RoutingKeys.ConsultaAgendada, agora.AddSeconds(1)));

        // Agir
        // 1. A primeira falha: por causa do mandatory: true, o RabbitMQ devolve, e o publicador lança exceção
        await RodarRelayAsync();

        // Verificar
        var mensagens = await OutboxAsync();                    // ordenadas por CriadoEm
        Assert.Equal(2, mensagens.Count);                       // 2.
        var (semDestino, seguinte) = (mensagens[0], mensagens[1]);

        // 3. A falha foi registrada
        Assert.Null(semDestino.PublicadoEm);
        Assert.Equal(1, semDestino.Tentativas);
        Assert.NotNull(semDestino.UltimoErro);

        // 4. O break parou o lote: a segunda não passou na frente da que falhou
        Assert.Null(seguinte.PublicadoEm);
        Assert.Equal(0, seguinte.Tentativas);

        Assert.Null(await fila.PegarAsync());                   // 5.
    }
}