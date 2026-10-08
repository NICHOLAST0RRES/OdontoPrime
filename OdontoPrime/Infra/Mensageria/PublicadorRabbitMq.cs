using System.Text;
using OdontoPrime.Infra.Mensageria.Outbox;
using RabbitMQ.Client;

namespace OdontoPrime.Infra.Mensageria;

public class PublicadorRabbitMq : IAsyncDisposable
{
    public const string NomeDaExchange = "clinica.eventos";
    private static readonly TimeSpan TempoMaximoDeConfirmacao = TimeSpan.FromSeconds(10);

    private readonly IConnection _conexao;
    private readonly IChannel _canal;

    private PublicadorRabbitMq(IConnection conexao, IChannel canal)
    {
        _conexao = conexao;
        _canal = canal;
    }

    public static async Task<PublicadorRabbitMq> CriarAsync(string connectionString, CancellationToken ct = default)
    {
        var fabrica = new ConnectionFactory
        {
            Uri = new Uri(connectionString),
            AutomaticRecoveryEnabled = false // quem reconecta é o relay
        };

        var conexao = await fabrica.CreateConnectionAsync(ct);

        // Liga a confirmação do broker e faz o BasicPublishAsync esperar por ela.
        var opcoesDoCanal = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);

        var canal = await conexao.CreateChannelAsync(opcoesDoCanal, ct);

        await canal.ExchangeDeclareAsync(
            exchange: NomeDaExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);

        return new PublicadorRabbitMq(conexao, canal);
    }

    public bool EstaAberto => _conexao.IsOpen && _canal.IsOpen;

    public async Task PublicarAsync(OutboxMensagem mensagem, CancellationToken ct = default)
    {
        var propriedades = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = mensagem.Id.ToString(),
            Type = mensagem.Tipo
        };

        using var limite = CancellationTokenSource.CreateLinkedTokenSource(ct);
        limite.CancelAfter(TempoMaximoDeConfirmacao);

        try
        {
            // Só termina quando o broker confirma. Recusa ou devolução viram exceção.
            await _canal.BasicPublishAsync(
                exchange: NomeDaExchange,
                routingKey: mensagem.RoutingKey,
                mandatory: true, // sem fila de destino: devolvida, e não descartada
                basicProperties: propriedades,
                body: Encoding.UTF8.GetBytes(mensagem.Payload),
                cancellationToken: limite.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            // Estourou o nosso limite; não foi a aplicação desligando.
            throw new TimeoutException(
                $"O broker não confirmou a mensagem {mensagem.Id} em {TempoMaximoDeConfirmacao.TotalSeconds:F0}s");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _canal.CloseAsync();
        await _conexao.CloseAsync();
        await _canal.DisposeAsync();
        await _conexao.DisposeAsync();
    }
}