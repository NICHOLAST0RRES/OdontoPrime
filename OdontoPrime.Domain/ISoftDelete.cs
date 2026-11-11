namespace OdontoPrime.Domain;

public interface ISoftDelete
{
    public bool Ativo { get;  }
    public DateTime? DeletadoEm { get;  }
}