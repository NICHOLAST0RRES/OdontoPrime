namespace OdontoPrime.Api.Dtos.Bloqueio;

// Início e fim em UTC: quem converte o horário da clínica é o cliente HTTP da tela.
public record BloqueioRequestDTO
{
    public Guid ProfissionalId { get; set; }
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public string Motivo { get; set; } = null!;
}
