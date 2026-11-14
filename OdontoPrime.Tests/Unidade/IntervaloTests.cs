using OdontoPrime.Domain.Models;

namespace OdontoPrime.Tests;

public class IntervaloTests
{
    // "10:00", "10:30" → 10h00–10h30 de um dia qualquer. Deixa os casos legíveis.
    private static Intervalo I(string inicio, string fim)
    {
        var dia = new DateTime(2026, 10, 12, 0, 0, 0, DateTimeKind.Utc);
        return new Intervalo(dia + TimeSpan.Parse(inicio), dia + TimeSpan.Parse(fim));
    }

    [Theory]                                                                    // 1.
    [InlineData("10:30", "10:00")]   // fim antes do início
    [InlineData("10:00", "10:00")]   // intervalo vazio
    public void Construtor_recusa_fim_antes_ou_igual_ao_inicio(string inicio, string fim)
    {
        Assert.Throws<ArgumentException>(() => I(inicio, fim));
    }

    [Theory]                                                                    // 2.
    [InlineData("10:00", "10:30", "10:30", "11:00", false)]   // encostados
    [InlineData("10:00", "10:30", "10:15", "10:45", true)]    // parcial
    [InlineData("10:00", "12:00", "10:30", "11:00", true)]    // um dentro do outro
    [InlineData("10:00", "10:30", "11:00", "11:30", false)]   // separados
    [InlineData("10:00", "10:30", "10:00", "10:30", true)]    // iguais
    public void Sobrepoe(string aInicio, string aFim, string bInicio, string bFim, bool esperado)
    {
        var a = I(aInicio, aFim);
        var b = I(bInicio, bFim);

        Assert.Equal(esperado, a.Sobrepoe(b));
        Assert.Equal(esperado, b.Sobrepoe(a));   // a regra tem que valer nos dois sentidos
    }

    [Theory]                                                                    // 3.
    [InlineData("08:00", "12:00", "10:00", "11:00", true)]    // cabe no meio
    [InlineData("08:00", "12:00", "11:30", "12:30", false)]   // passa do fim
    [InlineData("08:00", "12:00", "08:00", "12:00", true)]    // igual: cabe
    public void Contem(string aInicio, string aFim, string bInicio, string bFim, bool esperado)
    {
        Assert.Equal(esperado, I(aInicio, aFim).Contem(I(bInicio, bFim)));
    }

    [Fact]                                                                      // 4.
    public void Subtrair_sem_sobreposicao_devolve_o_intervalo_inteiro()
    {
        Assert.Equal([I("08:00", "12:00")], I("08:00", "12:00").Subtrair(I("12:00", "13:00")));
    }

    [Fact]
    public void Subtrair_o_meio_deixa_dois_pedacos()
    {
        Assert.Equal([I("08:00", "10:00"), I("11:00", "12:00")], I("08:00", "12:00").Subtrair(I("10:00", "11:00")));
    }

    [Fact]
    public void Subtrair_o_comeco_deixa_o_final()
    {
        Assert.Equal([I("09:00", "12:00")], I("08:00", "12:00").Subtrair(I("07:00", "09:00")));
    }

    [Fact]
    public void Subtrair_tudo_nao_deixa_nada()
    {
        Assert.Empty(I("10:00", "11:00").Subtrair(I("08:00", "12:00")));
    }
}