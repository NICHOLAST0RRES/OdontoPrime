namespace OdontoPrime.Api.Dtos.Jornada;

// Um período da jornada semanal, em horário de parede: "segunda, 08:00–12:00".
public record PeriodoDeTrabalhoDTO
{
    public DayOfWeek Dia { get; set; }
    public TimeOnly Inicio { get; set; }
    public TimeOnly Fim { get; set; }
}
