using OdontoPrime.Application;

namespace OdontoPrime.Infra.Mensageria.Outbox;
using Microsoft.EntityFrameworkCore;
using OdontoPrime.Data;


public class ManutencaoDaOutbox : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan Retencao = TimeSpan.FromDays(7);
    private static readonly TimeSpan AtrasoAceitavel = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ManutencaoDaOutbox> _logger;
    private readonly RelogioDaClinica _relogio; 

    public ManutencaoDaOutbox(IServiceScopeFactory scopeFactory, ILogger<ManutencaoDaOutbox> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Intervalo);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await AvisarSeHouverAtrasoAsync(context, stoppingToken);
                await ApagarPublicadasAntigasAsync(context, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Falha na manutenção da outbox");
            }
        }
    }

    private async Task AvisarSeHouverAtrasoAsync(AppDbContext context, CancellationToken ct)
    {
        var maisAntiga = await context.OutboxMensagens
            .Where(m => m.PublicadoEm == null)
            .OrderBy(m => m.CriadoEm)
            .Select(m => (DateTime?)m.CriadoEm)
            .FirstOrDefaultAsync(ct);

        if (maisAntiga is null) return;

        var atraso = _relogio.AgoraUtc() - maisAntiga.Value;
        if (atraso > AtrasoAceitavel)
        {
            _logger.LogWarning(
                "Outbox atrasada: a mensagem pendente mais antiga espera há {Minutos:F0} minutos",
                atraso.TotalMinutes);
        }
    }

    private async Task ApagarPublicadasAntigasAsync(AppDbContext context, CancellationToken ct)
    {
        var limite = _relogio.AgoraUtc() - Retencao;

        var apagadas = await context.OutboxMensagens
            .Where(m => m.PublicadoEm != null && m.PublicadoEm < limite)
            .ExecuteDeleteAsync(ct);

        if (apagadas > 0)
        {
            _logger.LogInformation(
                "{Total} mensagens publicadas há mais de {Dias} dias foram apagadas",
                apagadas, Retencao.TotalDays);
        }
    }
}