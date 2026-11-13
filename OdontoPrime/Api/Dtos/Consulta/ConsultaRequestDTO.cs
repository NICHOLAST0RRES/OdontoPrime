namespace OdontoPrime.Api.Dtos.Consulta;

public record ConsultaRequestDTO
{
    public Guid PacienteId { get; set; }
    public Guid ProfissionalId { get; set; }
    public DateTime DataHora { get; set; }
    public string? Observacao { get; set; }
    public int TipoAtendimentoId { get; set; }
    public int? DuracaoMinutos { get; set; }   // vazio = duração padrão do tipo
}