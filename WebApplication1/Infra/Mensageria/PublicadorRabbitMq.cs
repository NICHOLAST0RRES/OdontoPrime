using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace WebApplication1.Infra.Mensageria;

public class PublicadorRabbitMq  : IPublicadorDeEventos, IAsyncDisposable
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
    
    public static async Task<PublicadorRabbitMq> CriarAsync(string connectionString)
    {
        var fabrica = new ConnectionFactory
        {                                                                  // Como se trata de um publicador precisa abrir a conexão e criar a Exchange 
            Uri = new Uri(connectionString)                                
        };

        var conexao = await fabrica.CreateConnectionAsync();
        var canal = await conexao.CreateChannelAsync();

        await canal.ExchangeDeclareAsync(
            exchange: NomeDaExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );

        return new PublicadorRabbitMq(conexao, canal);
    }
    
    public async Task PublicarAsync<T>(T evento, string routingKey, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(evento , OpcoesJson);
        var corpo = Encoding.UTF8.GetBytes(json);        

        var propriedades = new BasicProperties
        {                                                      // Pega um objeto C# e coloca ele na exchange do RabbitMQ.
            Persistent = true,
            ContentType = "application/json",
            MessageId = Guid.CreateVersion7().ToString()
        };

        await _canal.BasicPublishAsync(
            exchange: NomeDaExchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: propriedades,
            body: corpo,
            cancellationToken: cancellationToken
        );
    }
    
    
    public async ValueTask DisposeAsync()
    {
        await _canal.CloseAsync();
        await _conexao.CloseAsync();
        await _canal.DisposeAsync();
        await _conexao.DisposeAsync();
    }
}