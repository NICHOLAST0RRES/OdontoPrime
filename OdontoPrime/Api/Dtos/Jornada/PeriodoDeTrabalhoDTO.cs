using System.Text.Json.Serialization;

namespace OdontoPrime.Api.Dtos.Jornada;

// Um período da jornada semanal, em horário de parede: "segunda, 08:00–12:00".
public record PeriodoDeTrabalhoDTO
{
    // A API escreve enum como texto ("Monday"). O conversor no próprio campo faz
    // quem lê o DTO (a tela, os testes) entender o mesmo formato, sem depender de configuração.
    [JsonConverter(typeof(JsonStringEnumConverter<DayOfWeek>))]
    public DayOfWeek Dia { get; set; }

    public TimeOnly Inicio { get; set; }
    public TimeOnly Fim { get; set; }
}
