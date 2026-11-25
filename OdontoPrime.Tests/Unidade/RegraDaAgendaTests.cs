using OdontoPrime.Domain.Models;

namespace OdontoPrime.Tests;

// Função: classificar um período (livre, fora da jornada ou bloqueado) direto na regra pura, sem banco.
// Protege: consulta aceita fora da jornada, no almoço, num dia sem jornada ou sobre um bloqueio;
// e um horário bloqueado sendo tratado como encaixável.
public class RegraDaAgendaTests
{
    // Jornada do dia: 08:00–12:00 e 14:00–18:00.
    private static readonly Intervalo[] Jornada = [I("08:00", "12:00"), I("14:00", "18:00")];
    private static readonly Intervalo[] SemBloqueios = [];

    private static Intervalo I(string inicio, string fim)
    {
        var dia = new DateTime(2026, 10, 12, 0, 0, 0, DateTimeKind.Utc);
        return new Intervalo(dia + TimeSpan.Parse(inicio), dia + TimeSpan.Parse(fim));
    }

    [Fact]
    public void Dentro_da_jornada_e_livre()
    {
        Assert.Equal(SituacaoDoHorario.Livre, RegraDaAgenda.Avaliar(I("10:00", "10:30"), Jornada, SemBloqueios));
    }

    [Fact]
    public void Terminar_depois_do_fim_da_jornada_e_fora()
    {
        Assert.Equal(SituacaoDoHorario.ForaDaJornada, RegraDaAgenda.Avaliar(I("11:45", "12:15"), Jornada, SemBloqueios));
    }

    [Fact]
    public void No_intervalo_de_almoco_e_fora()
    {
        Assert.Equal(SituacaoDoHorario.ForaDaJornada, RegraDaAgenda.Avaliar(I("12:30", "13:00"), Jornada, SemBloqueios));
    }

    [Fact]
    public void Dia_sem_jornada_e_fora()
    {
        Assert.Equal(SituacaoDoHorario.ForaDaJornada, RegraDaAgenda.Avaliar(I("10:00", "10:30"), [], SemBloqueios));
    }

    [Fact]
    public void Sobre_bloqueio_dentro_da_jornada_e_bloqueado()
    {
        Assert.Equal(SituacaoDoHorario.Bloqueado,
            RegraDaAgenda.Avaliar(I("10:00", "10:30"), Jornada, [I("09:00", "11:00")]));
    }

    [Fact]
    public void Fora_da_jornada_e_sobre_bloqueio_e_bloqueado()
    {
        Assert.Equal(SituacaoDoHorario.Bloqueado,
            RegraDaAgenda.Avaliar(I("19:00", "19:30"), Jornada, [I("18:00", "20:00")]));
    }

    [Fact]
    public void Encostado_no_bloqueio_e_livre()
    {
        Assert.Equal(SituacaoDoHorario.Livre,
            RegraDaAgenda.Avaliar(I("11:00", "11:30"), Jornada, [I("09:00", "11:00")]));
    }
}
