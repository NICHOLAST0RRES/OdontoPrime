namespace OdontoPrime.Domain.Models;

public class Bloqueio : IAuditavel
{
    public const int TamanhoMaximoDoMotivo = 200;

    public Guid Id { get; private set; }
    public Guid ProfissionalId { get; private set; }
    public DateTime Inicio { get; private set; }
    public DateTime Fim { get; private set; }
    public string Motivo { get; private set; } = null!;

    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    public Intervalo Periodo => new(Inicio, Fim);

    private Bloqueio() { }   // usado pelo EF

    public Bloqueio(Guid profissionalId, DateTime inicio, DateTime fim, string motivo, DateTime agoraUtc)
    {
        var periodo = new Intervalo(inicio, fim);   // recusa fim antes ou igual ao início

        if (periodo.Fim <= agoraUtc)
            throw new ArgumentException("Esse bloqueio já terminou.", nameof(fim));

        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("Informe o motivo do bloqueio.", nameof(motivo));

        if (motivo.Trim().Length > TamanhoMaximoDoMotivo)
            throw new ArgumentException($"O motivo pode ter no máximo {TamanhoMaximoDoMotivo} caracteres.", nameof(motivo));

        Id = Guid.CreateVersion7();
        ProfissionalId = profissionalId;
        Inicio = periodo.Inicio;
        Fim = periodo.Fim;
        Motivo = motivo.Trim();
    }
    
}