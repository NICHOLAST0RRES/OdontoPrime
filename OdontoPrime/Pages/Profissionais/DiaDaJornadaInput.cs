namespace OdontoPrime.Pages.Profissionais;

// Uma linha da tela de jornada: um dia com até dois períodos (normalmente manhã e tarde).
// Período em branco = o dentista não atende nesse período.
public class DiaDaJornadaInput
{
    public DayOfWeek Dia { get; set; }

    public TimeOnly? Inicio1 { get; set; }
    public TimeOnly? Fim1 { get; set; }

    public TimeOnly? Inicio2 { get; set; }
    public TimeOnly? Fim2 { get; set; }
}
