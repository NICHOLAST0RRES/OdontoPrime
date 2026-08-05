namespace WebApplication1.Api.Dtos.TipoProfissional;

public record ProfissionalRequestDTO
{
    public string Nome { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public int TipoProfissionalId { get; set; }
    public string? Cro { get; set; }
}