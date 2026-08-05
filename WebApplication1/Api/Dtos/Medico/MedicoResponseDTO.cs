namespace WebApplication1.Api.Dtos.Medico;

public record MedicoResponseDTO
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Crm { get; set; }
    public DateOnly DataNascimento { get; set; }
    public int EspecialidadeId { get; set; }
    public string EspecialidadeNome { get; set; } = string.Empty;

}