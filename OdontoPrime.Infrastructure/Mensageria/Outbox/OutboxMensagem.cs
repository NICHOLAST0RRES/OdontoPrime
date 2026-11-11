using System.Text.Json;

namespace OdontoPrime.Infra.Mensageria.Outbox;

public class OutboxMensagem
{
    public Guid Id { get; private set; }
    public string Tipo { get; private set; } = null!;
    public string RoutingKey { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTime CriadoEm { get; private set; }
    public DateTime? PublicadoEm { get; private set; }
    public int Tentativas { get; private set; }
    public string? UltimoErro { get; private set; }

    private OutboxMensagem() { } // EF

    public static OutboxMensagem Criar(object evento, string routingKey , DateTime agoraUtc)
    {
        var tipo = evento.GetType();

        return new OutboxMensagem
        {
            Id = Guid.CreateVersion7(), // vira o MessageId: nasce com o evento, não na publicação
            Tipo = tipo.Name,
            RoutingKey = routingKey,
            Payload = JsonSerializer.Serialize(evento, tipo, OpcoesJsonMensageria.Padrao),
            CriadoEm = agoraUtc
        };
    }

    public void MarcarComoPublicada(DateTime agoraUtc) => PublicadoEm = agoraUtc;

    public void RegistrarFalha(string erro)
    {
        Tentativas++;
        UltimoErro = erro.Length > 2000 ? erro[..2000] : erro;
    }
}