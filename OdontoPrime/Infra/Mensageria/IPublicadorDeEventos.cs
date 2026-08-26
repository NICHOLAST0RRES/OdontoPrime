namespace OdontoPrime.Infra.Mensageria;

public interface IPublicadorDeEventos
{
    Task PublicarAsync<T>(T evento, string routingKey, CancellationToken cancellationToken = default);
}