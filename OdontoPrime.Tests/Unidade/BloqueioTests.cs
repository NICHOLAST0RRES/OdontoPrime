using OdontoPrime.Domain.Models;

namespace OdontoPrime.Tests;

public class BloqueioTests
{
    private static readonly DateTime Agora = new(2026, 10, 9, 13, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Inicio = new(2026, 10, 20, 11, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Fim = new(2026, 10, 20, 21, 0, 0, DateTimeKind.Utc);

    private static Bloqueio NovoBloqueio(DateTime inicio, DateTime fim, string motivo = "Congresso")
        => new(Guid.NewGuid(), inicio, fim, motivo, Agora);

    [Fact]
    public void Recusa_fim_antes_do_inicio()
    {
        Assert.Throws<ArgumentException>(() => NovoBloqueio(Fim, Inicio));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Recusa_motivo_em_branco(string motivo)
    {
        Assert.Throws<ArgumentException>(() => NovoBloqueio(Inicio, Fim, motivo));
    }

    [Fact]
    public void Recusa_bloqueio_que_ja_terminou()
    {
        Assert.Throws<ArgumentException>(() => NovoBloqueio(Agora.AddDays(-2), Agora.AddDays(-1)));
    }

    [Fact]
    public void Guarda_o_periodo_e_o_motivo_sem_espacos()
    {
        var bloqueio = NovoBloqueio(Inicio, Fim, "  Congresso  ");

        Assert.Equal(new Intervalo(Inicio, Fim), bloqueio.Periodo);
        Assert.Equal("Congresso", bloqueio.Motivo);
    }
}