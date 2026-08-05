namespace WebApplication1.Api.Dtos.Convenio;

public record ConvenioResponseDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
}
