using Clinica.Contratos;
using Microsoft.EntityFrameworkCore;
using OdontoPrime.Application;
using OdontoPrime.Data;
using OdontoPrime.Domain.Models;
using OdontoPrime.Infra.Mensageria;

namespace OdontoPrime.Infra.Jobs;

public class LembreteScheduler  : BackgroundService
{
   private readonly IServiceProvider _serviceProvider;
   private readonly ILogger<LembreteScheduler> _logger;
   private  readonly RelogioDaClinica _relogioDaClinica;
   private DateTimeOffset NaClinica(DateTime utc) => _relogioDaClinica.ParaOffsetDaClinica(utc);

   

    public LembreteScheduler(IServiceProvider serviceProvider, ILogger<LembreteScheduler> logger, RelogioDaClinica relogioDaClinica)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _relogioDaClinica = relogioDaClinica;
    }

    

    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await GerarLembretesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao gerar lembretes");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task GerarLembretesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var outbox = scope.ServiceProvider.GetRequiredService<IOutbox>(); // mesmo escopo = mesmo DbContext

        var amanhaNaClinica = _relogioDaClinica.HojeNaClinica().AddDays(1);
        var inicio = _relogioDaClinica.ParaUtc(amanhaNaClinica);
        var fim = _relogioDaClinica.ParaUtc(amanhaNaClinica.AddDays(1));

        var consultas = await context.Consultas
            .Include(c => c.Paciente)
            .Include(c => c.Profissional)
            .Where(c => c.StatusConsultaId == StatusConsulta.AgendadaId)
            .Where(c => c.LembreteEnviadoEm == null)
            .Where(c => c.DataHora >= inicio && c.DataHora < fim)
            .ToListAsync(ct);

        var agora = _relogioDaClinica.AgoraUtc();

        foreach (var consulta in consultas)
        {
            outbox.Adicionar(new LembreteDeConsulta(
                consulta.Id,
                consulta.Paciente.Nome,
                consulta.Paciente.Telefone,
                consulta.Profissional.Nome,
                NaClinica(consulta.DataHora),
                NaClinica(agora)
            ), RoutingKeys.LembreteDeConsulta);

            consulta.MarcarLembreteEnviado(agora);
        }

        await context.SaveChangesAsync(ct); // marcações + mensagens: tudo ou nada

        if (consultas.Count > 0)
        {
            _logger.LogInformation("{Total} lembretes gerados", consultas.Count);
        }
    }
    
    
    
}