using OdontoPrime.Application;
using OdontoPrime.Data;

namespace OdontoPrime.Infra.Mensageria.Outbox;

public class OutboxEfCore : IOutbox
{
    private readonly AppDbContext _context;
    private readonly TimeProvider _tempo;

    public OutboxEfCore(AppDbContext context, TimeProvider tempo)
    {
        _context = context;
        _tempo = tempo;
    }

    public void Adicionar(object evento, string routingKey)
        => _context.OutboxMensagens.Add(
            OutboxMensagem.Criar(evento, routingKey, _tempo.GetUtcNow().UtcDateTime));
}