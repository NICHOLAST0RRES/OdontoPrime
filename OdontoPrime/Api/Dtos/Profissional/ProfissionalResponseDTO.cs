namespace OdontoPrime.Api.Dtos.TipoProfissional;

public class ProfissionalResponseDTO
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string TipoProfissional { get; set; } = null!;
    public string? Cro { get; set; }
}