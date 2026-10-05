namespace OdontoPrime.Application;

// interface que recebe qualquer evento 
public interface IOutbox
{
    void Adicionar(object evento, string routingKey);
}