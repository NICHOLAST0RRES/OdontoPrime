namespace WebApplication1.Domain.Models;

public class Profissional  : IAuditavel, ISoftDelete
{
    public Guid Id { get; private set; } 
    public string Nome { get; private set; }
    public string Telefone { get; private set; }
    public int TipoProfissionalId { get; private set; }
    public TipoProfissional TipoProfissional { get; private set; }
    public string? Cro { get; private set; }

    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime? DeletadoEm { get; private set; }

    private Profissional()
    {
        
    }
    
    public Profissional(string nome, string telefone, int tipoProfissionalId, string? cro)
    {
        if (tipoProfissionalId == TipoProfissional.DentistaId && string.IsNullOrWhiteSpace(cro))
        {
            throw new ArgumentException("Dentista precisa de CRO.", nameof(cro));
        }

        if (tipoProfissionalId == TipoProfissional.AtendenteId && !string.IsNullOrWhiteSpace(cro))
        {
            throw new ArgumentException("Atendente não tem CRO.", nameof(cro));
        }

        Id = Guid.CreateVersion7();
        Nome = nome;
        Telefone = telefone;
        TipoProfissionalId = tipoProfissionalId;
        Cro = cro;
        Ativo = true;
    }
    
    public void Atualizar(string nome, string telefone)
    {
        Nome = nome;
        Telefone = telefone;
    }

    public void Desativar()
    {
        Ativo = false;
        DeletadoEm = DateTime.UtcNow;
    }

    public void Reativar()
    {
        Ativo = true;
        DeletadoEm = null;
    }
    
}