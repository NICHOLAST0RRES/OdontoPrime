using OdontoPrime.Infra.Mensageria;
using RabbitMQ.Client;

namespace OdontoPrime.Tests;

// Uma fila temporária no RabbitMQ do container, para o teste ver o que o relay publicou.
public sealed class FilaDeTeste : IAsyncDisposable
{
    private readonly IConnection _conexao;
    private readonly IChannel _canal;
    private readonly string _nome;

    private FilaDeTeste(IConnection conexao, IChannel canal, string nome)
    {
        _conexao = conexao;
        _canal = canal;
        _nome = nome;
    }

    public static async Task<FilaDeTeste> CriarAsync(string connectionString, params string[] padroes)
    {
        var fabrica = new ConnectionFactory { Uri = new Uri(connectionString) };
        var conexao = await fabrica.CreateConnectionAsync();
        var canal = await conexao.CreateChannelAsync();

        await canal.ExchangeDeclareAsync(
            exchange: PublicadorRabbitMq.NomeDaExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        // Nome gerado pelo broker e exclusiva desta conexão: some sozinha quando o teste termina.
        var fila = await canal.QueueDeclareAsync(queue: "", durable: false, exclusive: true, autoDelete: true);

        foreach (var padrao in padroes)
        {
            await canal.QueueBindAsync(fila.QueueName, PublicadorRabbitMq.NomeDaExchange, padrao);
        }

        return new FilaDeTeste(conexao, canal, fila.QueueName);
    }

    // Pega uma mensagem, se houver. Não espera: devolve null se a fila estiver vazia.
    public Task<BasicGetResult?> PegarAsync() => _canal.BasicGetAsync(_nome, autoAck: true);

    public async ValueTask DisposeAsync()
    {
        await _canal.CloseAsync();
        await _conexao.CloseAsync();
        await _canal.DisposeAsync();
        await _conexao.DisposeAsync();
    }
}