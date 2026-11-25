namespace OdontoPrime.Domain.Models;

public class PeriodoDeTrabalho
{
    public const int PassoEmMinutos = 15;

    public DayOfWeek Dia { get; private set; }
    public TimeOnly Inicio { get; private set; }
    public TimeOnly Fim { get; private set; }

    private PeriodoDeTrabalho() { }   // usado pelo EF

    public PeriodoDeTrabalho(DayOfWeek dia, TimeOnly inicio, TimeOnly fim)
    {
        if (fim <= inicio)
            throw new ArgumentException($"{NomeDoDia(dia)}: o fim do período tem que ser depois do início.", nameof(fim));

        if (inicio.Minute % PassoEmMinutos != 0 || fim.Minute % PassoEmMinutos != 0)
            throw new ArgumentException($"{NomeDoDia(dia)}: os horários da jornada têm que ser múltiplos de {PassoEmMinutos} minutos.");

        Dia = dia;
        Inicio = inicio;
        Fim = fim;
    }

    public bool Sobrepoe(PeriodoDeTrabalho outro)
        => Dia == outro.Dia && Inicio < outro.Fim && outro.Inicio < Fim;

    // Nome do dia em português, pras mensagens e pra tela.
    public static string NomeDoDia(DayOfWeek dia) => dia switch
    {
        DayOfWeek.Monday => "Segunda",
        DayOfWeek.Tuesday => "Terça",
        DayOfWeek.Wednesday => "Quarta",
        DayOfWeek.Thursday => "Quinta",
        DayOfWeek.Friday => "Sexta",
        DayOfWeek.Saturday => "Sábado",
        _ => "Domingo"
    };
}