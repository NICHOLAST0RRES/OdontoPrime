namespace WebApplication1.Domain;

public interface ISoftDelete
{
    public bool Ativo { get;  }
    public DateTime? DeletadoEm { get;  }
}