namespace OdontoPrime.Api.Dtos.TipoProfissional;

public record TipoProfissionalResponseDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
}
