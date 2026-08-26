using Clinica.Contratos;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Models;
using WebApplication1.Infra.Mensageria;

namespace WebApplication1.Infra.Jobs;

public class LembreteScheduler  : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPublicadorDeEventos _publicador;
    private readonly ILogger<LembreteScheduler> _logger;

    public LembreteScheduler(
        IServiceProvider serviceProvider,
        IPublicadorDeEventos publicador,
        ILogger<LembreteScheduler> logger)
    {
        _serviceProvider = serviceProvider;
        _publicador = publicador;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublicarLembretesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao publicar lembretes");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task PublicarLembretesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var inicio = DateTime.UtcNow.Date.AddDays(1);
        var fim = inicio.AddDays(1);

        var consultas = await context.Consultas
            .Include(c => c.Paciente)
            .Include(c => c.Profissional)
            .Where(c => c.StatusConsultaId == StatusConsulta.AgendadaId)
            .Where(c => c.LembreteEnviadoEm == null)
            .Where(c => c.DataHora >= inicio && c.DataHora < fim)
            .ToListAsync(ct);

        foreach (var consulta in consultas)
        {
            var evento = new LembreteDeConsulta(
                consulta.Id,
                consulta.Paciente.Nome,
                consulta.Paciente.Telefone,
                consulta.Profissional.Nome,
                consulta.DataHora
            );

            await _publicador.PublicarAsync(evento, RoutingKeys.LembreteDeConsulta, ct);
            consulta.MarcarLembreteEnviado();
        }
        
        await context.SaveChangesAsync(ct);

        if (consultas.Count > 0)
        {
            _logger.LogInformation("{Total} lembretes publicados", consultas.Count);
        }
    }
    
}