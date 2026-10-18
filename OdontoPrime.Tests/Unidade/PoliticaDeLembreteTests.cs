using Microsoft.Extensions.Configuration;
using OdontoPrime.Application;
using Xunit;

namespace OdontoPrime.Tests;

public class PoliticaDeLembreteTests
{
    
    private readonly PoliticaDeLembrete _politica;

    public PoliticaDeLembreteTests()
    {
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Clinica:FusoHorario"] = "America/Bahia",
                ["Clinica:HoraDoLembrete"] = "18"
            })
            .Build();

        var relogio = new RelogioDaClinica(TimeProvider.System, configuracao);
        _politica = new PoliticaDeLembrete(relogio, configuracao);
    }

    // Um horário de Salvador convertido para UTC, do jeito que fica gravado no banco.
    private static DateTime NaClinica(int mes, int dia, int hora, int minuto = 0)
        => new DateTimeOffset(2026, mes, dia, hora, minuto, 0, TimeSpan.FromHours(-3)).UtcDateTime;

    [Fact]
    public void Lembrete_sai_na_vespera_as_18h()
    {
        var agora = NaClinica(10, 9, 10);       // sexta, 10h
        var consulta = NaClinica(10, 11, 10);   // domingo, 10h

        var lembrarEm = _politica.CalcularLembrarEm(consulta, agora);

        Assert.Equal(NaClinica(10, 10, 18), lembrarEm);   // sábado, 18h
    }

    [Fact]
    public void Consulta_tarde_da_noite_usa_o_dia_de_Salvador_e_nao_o_do_UTC()
    {
        var agora = NaClinica(10, 9, 10);
        var consulta = NaClinica(10, 12, 23, 30);   // segunda 23h30 em Salvador = terça 02h30 em UTC

        var lembrarEm = _politica.CalcularLembrarEm(consulta, agora);

        Assert.Equal(NaClinica(10, 11, 18), lembrarEm);   // domingo 18h, e não segunda
    }

    [Fact]
    public void Sem_lembrete_quando_a_vespera_as_18h_ja_passou()
    {
        var agora = NaClinica(10, 10, 19);      // sábado, 19h
        var consulta = NaClinica(10, 11, 10);   // domingo, 10h

        var lembrarEm = _politica.CalcularLembrarEm(consulta, agora);

        Assert.Null(lembrarEm);
    }
}