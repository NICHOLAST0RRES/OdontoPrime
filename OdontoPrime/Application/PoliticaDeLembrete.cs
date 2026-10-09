namespace OdontoPrime.Application;

public class PoliticaDeLembrete
{
    private readonly RelogioDaClinica _relogio;
    private readonly TimeSpan _horario;

    public PoliticaDeLembrete(RelogioDaClinica relogio, IConfiguration configuration)
    {
        _relogio = relogio;
        _horario = TimeSpan.FromHours(configuration.GetValue("Clinica:HoraDoLembrete", 18));
    }

    // Devolve o instante (UTC) em que o lembrete deve sair, ou null se não faz mais sentido lembrar.
    public DateTime? CalcularLembrarEm(DateTime dataHoraUtc, DateTime agoraUtc)
    {
        var diaDaConsulta = _relogio.ParaHorarioDaClinica(dataHoraUtc).Date;
        var lembrarNaClinica = diaDaConsulta.AddDays(-1).Add(_horario);   // véspera, 18h em Salvador
        var lembrarUtc = _relogio.ParaUtc(lembrarNaClinica);

        return lembrarUtc > agoraUtc ? lembrarUtc : null;
    }
}