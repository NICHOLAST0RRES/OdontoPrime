namespace WebApplication1.Api.Dtos.Consulta;

public record ConsultaRequestDTO
{
    public Guid PacienteId { get; set; }
    public Guid ProfissionalId { get; set; }
    public DateTime DataHora { get; set; }
    public string? Observacao { get; set; }
}