namespace OdontoPrime.Application;

public class RelogioDaClinica
{
    private readonly TimeProvider _tempo;

    public TimeZoneInfo Fuso { get; }

    public RelogioDaClinica(TimeProvider tempo, IConfiguration configuration)
    {
        _tempo = tempo;
        Fuso = TimeZoneInfo.FindSystemTimeZoneById(
            configuration["Clinica:FusoHorario"] ?? "America/Bahia");
    }

    // O instante atual, em UTC. É o que se grava no banco e se usa em comparações.
    public DateTime AgoraUtc() => _tempo.GetUtcNow().UtcDateTime;

    // O que o relógio da parede da clínica marca agora.
    public DateTime AgoraNaClinica() => TimeZoneInfo.ConvertTimeFromUtc(AgoraUtc(), Fuso);

    public DateTime HojeNaClinica() => AgoraNaClinica().Date;

    // Um horário digitado na clínica ("15/10 às 10h") vira o instante em UTC.
    public DateTime ParaUtc(DateTime horarioDaClinica)
    {
        if (horarioDaClinica.Kind == DateTimeKind.Utc) return horarioDaClinica;

        var semFuso = DateTime.SpecifyKind(horarioDaClinica, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(semFuso, Fuso);
    }

    // Um instante em UTC vira o horário mostrado na clínica.
    public DateTime ParaHorarioDaClinica(DateTime utc)
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Fuso);

    // O instante com o offset da clínica (-03:00). Usado nos contratos, na Parte 2.
    public DateTimeOffset ParaOffsetDaClinica(DateTime utc)
        => TimeZoneInfo.ConvertTime(
            new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)), Fuso);
}