namespace OdontoPrime.Domain.Models;

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
    
    private readonly List<PeriodoDeTrabalho> _jornada = [];
    public IReadOnlyCollection<PeriodoDeTrabalho> Jornada => _jornada;

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
    
    public void DefinirJornada(IEnumerable<PeriodoDeTrabalho> periodos)
    {
        if (TipoProfissionalId != TipoProfissional.DentistaId)
            throw new InvalidOperationException("Só dentista tem jornada.");

        var novos = periodos.ToList();

        for (var i = 0; i < novos.Count; i++)
        for (var j = i + 1; j < novos.Count; j++)
        {
            if (novos[i].Sobrepoe(novos[j]))
                throw new ArgumentException($"{PeriodoDeTrabalho.NomeDoDia(novos[i].Dia)}: há períodos sobrepostos na jornada.");
        }

        _jornada.Clear();
        _jornada.AddRange(novos);
    }

// A jornada de uma data, convertida em instantes UTC com o fuso da clínica.
    public IReadOnlyList<Intervalo> JornadaEm(DateOnly data, TimeZoneInfo fuso)
    {
        return _jornada
            .Where(p => p.Dia == data.DayOfWeek)
            .OrderBy(p => p.Inicio)
            .Select(p => new Intervalo(
                TimeZoneInfo.ConvertTimeToUtc(data.ToDateTime(p.Inicio), fuso),
                TimeZoneInfo.ConvertTimeToUtc(data.ToDateTime(p.Fim), fuso)))
            .ToList();
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