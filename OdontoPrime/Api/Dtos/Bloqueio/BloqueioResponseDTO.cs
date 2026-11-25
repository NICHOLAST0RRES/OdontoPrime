namespace OdontoPrime.Api.Dtos.Bloqueio;

// Início e fim em UTC. A tela converte pro horário da clínica na hora de mostrar.
public record BloqueioResponseDTO
{
    public Guid Id { get; set; }
    public Guid ProfissionalId { get; set; }
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public string Motivo { get; set; } = null!;
}
