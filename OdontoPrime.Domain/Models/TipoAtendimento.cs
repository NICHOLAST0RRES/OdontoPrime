namespace OdontoPrime.Domain.Models;

public class TipoAtendimento
{
    public const int AvaliacaoId = 1;
    public const int LimpezaId = 2;
    public const int RestauracaoId = 3;
    public const int CanalId = 4;
    public const int UrgenciaId = 5;

    public const int PassoEmMinutos = 15;

    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public int DuracaoPadraoMinutos { get; set; }
    public int DuracaoMaximaMinutos { get; set; }

    public void ValidarDuracao(int minutos)
    {
        if (minutos <= 0 || minutos % PassoEmMinutos != 0)
            throw new ArgumentException($"A duração tem que ser múltipla de {PassoEmMinutos} minutos.", nameof(minutos));

        if (minutos > DuracaoMaximaMinutos)
            throw new ArgumentException($"{Nome} pode durar no máximo {DuracaoMaximaMinutos} minutos.", nameof(minutos));
    }
}