using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OdontoPrime.Data;
using OdontoPrime.Domain.Models;
using Xunit;

namespace OdontoPrime.Tests;

public abstract class TesteDeIntegracao(InfraDeTeste infra) : IAsyncLifetime
{
    protected InfraDeTeste Infra { get; } = infra;
    protected RelogioDeTeste Relogio => Infra.Relogio;
    protected IServiceScope NovoEscopo() => Infra.App.Services.CreateScope();
    
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    protected static DateTimeOffset NaClinica(int mes, int dia, int hora, int minuto = 0)
        => new(2026, mes, dia, hora, minuto, 0, TimeSpan.FromHours(-3));
    

    public async ValueTask InitializeAsync()
    {
        using var escopo = NovoEscopo();
        var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.ExecuteSqlRawAsync(
            """TRUNCATE "OutboxMensagens", "Consultas", "Pacientes", "Profissionais", "Convenios" RESTART IDENTITY CASCADE""");

        Relogio.Ajustar(NaClinica(10, 9, 10)); // padrão: sexta, 09/10, 10h em Salvador
    }
    

    protected async Task<(Guid PacienteId, Guid DentistaId)> CriarPacienteEDentistaAsync()
    {
        using var escopo = NovoEscopo();
        var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();

        var convenio = new Convenio { Nome = "Particular" };
        db.Convenios.Add(convenio);
        await db.SaveChangesAsync();

        var paciente = new Paciente("Maria Teste", "52998224725", "71999990000", "maria@teste.com", convenio.Id);
        var dentista = new Profissional("Dr. Teste", "71988880000", TipoProfissional.DentistaId, "CRO-BA 1234");
        db.AddRange(paciente, dentista);
        await db.SaveChangesAsync();

        return (paciente.Id, dentista.Id);
    }
}