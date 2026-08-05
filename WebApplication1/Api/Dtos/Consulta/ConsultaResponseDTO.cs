namespace WebApplication1.Api.Dtos.Consulta;

public record ConsultaResponseDTO
{
    public Guid Id { get; set; }
    public Guid PacienteId { get; set; }
    public string PacienteNome { get; set; } = null!;
    public Guid ProfissionalId { get; set; }
    public string ProfissionalNome { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime DataHora { get; set; }
    public string? Observacao { get; set; }
}