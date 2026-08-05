namespace WebApplication1.Domain.Models;

public class StatusConsulta
{
    public const int AgendadaId = 1;
    public const int RealizadaId = 2;
    public const int CanceladaId = 3;

    public int Id { get; set; }
    public string Nome { get; set; } = null!;
}