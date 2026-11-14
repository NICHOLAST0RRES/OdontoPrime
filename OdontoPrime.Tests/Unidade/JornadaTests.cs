using OdontoPrime.Domain.Models;

namespace OdontoPrime.Tests;

public class JornadaTests
{
     private static readonly TimeZoneInfo Bahia = TimeZoneInfo.FindSystemTimeZoneById("America/Bahia");
    private static readonly DateOnly Segunda = new(2026, 10, 12);

    private static PeriodoDeTrabalho P(DayOfWeek dia, string inicio, string fim)
        => new(dia, TimeOnly.Parse(inicio), TimeOnly.Parse(fim));

    private static Profissional NovoDentista()
        => new("Dra. Ana", "71900000000", TipoProfissional.DentistaId, "CRO-BA 1111");

    // Período

    [Theory]
    [InlineData("12:00", "08:00")]   // fim antes do início
    [InlineData("08:00", "08:00")]   // fim igual ao início
    public void Periodo_recusa_fim_antes_ou_igual_ao_inicio(string inicio, string fim)
    {
        Assert.Throws<ArgumentException>(() => P(DayOfWeek.Monday, inicio, fim));
    }

    [Fact]
    public void Periodo_recusa_horario_fora_do_passo_de_15_minutos()
    {
        Assert.Throws<ArgumentException>(() => P(DayOfWeek.Monday, "08:10", "12:00"));
    }

    // Jornada

    [Fact]
    public void Aceita_manha_e_tarde_no_mesmo_dia()
    {
        var dentista = NovoDentista();

        dentista.DefinirJornada([P(DayOfWeek.Monday, "08:00", "12:00"), P(DayOfWeek.Monday, "14:00", "18:00")]);

        Assert.Equal(2, dentista.Jornada.Count);
    }

    [Fact]
    public void Recusa_periodos_sobrepostos_no_mesmo_dia()
    {
        var dentista = NovoDentista();

        Assert.Throws<ArgumentException>(() =>
            dentista.DefinirJornada([P(DayOfWeek.Monday, "08:00", "12:00"), P(DayOfWeek.Monday, "11:00", "14:00")]));
    }

    [Fact]
    public void Aceita_o_mesmo_horario_em_dias_diferentes()
    {
        var dentista = NovoDentista();

        dentista.DefinirJornada([P(DayOfWeek.Monday, "08:00", "12:00"), P(DayOfWeek.Tuesday, "08:00", "12:00")]);

        Assert.Equal(2, dentista.Jornada.Count);
    }

    [Fact]
    public void Atendente_nao_tem_jornada()
    {
        var atendente = new Profissional("João", "71900000001", TipoProfissional.AtendenteId, cro: null);

        Assert.Throws<InvalidOperationException>(() =>
            atendente.DefinirJornada([P(DayOfWeek.Monday, "08:00", "12:00")]));
    }

    [Fact]
    public void Definir_de_novo_substitui_a_jornada_anterior()
    {
        var dentista = NovoDentista();
        dentista.DefinirJornada([P(DayOfWeek.Monday, "08:00", "12:00"), P(DayOfWeek.Tuesday, "08:00", "12:00")]);

        dentista.DefinirJornada([P(DayOfWeek.Friday, "14:00", "18:00")]);

        var unico = Assert.Single(dentista.Jornada);
        Assert.Equal(DayOfWeek.Friday, unico.Dia);
    }

    // JornadaEm

    [Fact]
    public void JornadaEm_converte_o_horario_de_parede_para_UTC()
    {
        var dentista = NovoDentista();
        dentista.DefinirJornada([P(DayOfWeek.Monday, "08:00", "12:00")]);

        var intervalo = Assert.Single(dentista.JornadaEm(Segunda, Bahia));

        Assert.Equal(new DateTime(2026, 10, 12, 11, 0, 0, DateTimeKind.Utc), intervalo.Inicio);   // 08h na Bahia
        Assert.Equal(new DateTime(2026, 10, 12, 15, 0, 0, DateTimeKind.Utc), intervalo.Fim);      // 12h na Bahia
    }

    [Fact]
    public void JornadaEm_um_dia_sem_jornada_devolve_vazio()
    {
        var dentista = NovoDentista();
        dentista.DefinirJornada([P(DayOfWeek.Monday, "08:00", "12:00")]);

        Assert.Empty(dentista.JornadaEm(Segunda.AddDays(1), Bahia));   // terça
    }
}