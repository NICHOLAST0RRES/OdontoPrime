using System.Text;
using System.Text.Json;
using OdontoPrime.Infra.Mensageria.Outbox;
using RabbitMQ.Client;

namespace OdontoPrime.Infra.Mensageria;

public class PublicadorRabbitMq  :  IAsyncDisposable
{
    
    public const string NomeDaExchange = "clinica.eventos";
    
    private static readonly JsonSerializerOptions OpcoesJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    
    private readonly IConnection _conexao;
    private readonly IChannel _canal;
    
    private PublicadorRabbitMq(IConnection conexao, IChannel canal)
    {
        _conexao = conexao;
        _canal = canal;
    }
    
    public static async Task<PublicadorRabbitMq> CriarAsync(string connectionString, CancellationToken ct = default)
    {
        var fabrica = new ConnectionFactory           // Como se trata de um publicador precisa abrir a conexão e criar a Exchange 
        {
            Uri = new Uri(connectionString),
            AutomaticRecoveryEnabled = false // quem reconecta é o relay
        }; 

        var conexao = await fabrica.CreateConnectionAsync(ct);
        var canal = await conexao.CreateChannelAsync(cancellationToken: ct);

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
            MessageId = mensagem.Id.ToString(), // o mesmo em toda tentativa
            Type = mensagem.Tipo
        };

        await _canal.BasicPublishAsync(
            exchange: NomeDaExchange,                    // Pega um objeto C# e coloca ele na exchange do RabbitMQ.

            routingKey: mensagem.RoutingKey,              
            mandatory: false, // vira true na Parte 2
            basicProperties: propriedades,
            body: Encoding.UTF8.GetBytes(mensagem.Payload),
            cancellationToken: ct);
    }

    public async ValueTask DisposeAsync()
    {
        await _canal.CloseAsync();
        await _conexao.CloseAsync();
        await _canal.DisposeAsync();
        await _conexao.DisposeAsync();
    }
}





