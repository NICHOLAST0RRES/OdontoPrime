namespace OdontoPrime.Api.Dtos.TipoAtendimento;

public record TipoAtendimentoResponseDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public int DuracaoPadraoMinutos { get; set; }
    public int DuracaoMaximaMinutos { get; set; }
}
