namespace OdontoPrime.Domain;

public interface IAuditavel
{
    public DateTime CriadoEm { get; }
    public DateTime? AtualizadoEm { get;  }
}