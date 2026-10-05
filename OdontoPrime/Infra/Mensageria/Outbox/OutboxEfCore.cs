using OdontoPrime.Application;
using OdontoPrime.Data;

namespace OdontoPrime.Infra.Mensageria.Outbox;

public class OutboxEfCore :  IOutbox
{
    private readonly AppDbContext _context;

    public OutboxEfCore(AppDbContext context) => _context = context;

    public void Adicionar(object evento, string routingKey)
        => _context.OutboxMensagens.Add(OutboxMensagem.Criar(evento, routingKey));
}