namespace WebApplication1.Domain.Models;

public class TipoProfissional
{
    public const int DentistaId = 1;
    public const int AtendenteId = 2;

    public int Id { get; set; }
    public string Nome { get; set; } = null!;
}