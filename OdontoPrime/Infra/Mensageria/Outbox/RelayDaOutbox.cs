using Microsoft.EntityFrameworkCore;
using OdontoPrime.Application;
using OdontoPrime.Data;

namespace OdontoPrime.Infra.Mensageria.Outbox;

public class RelayDaOutbox : BackgroundService
{
    private const int TamanhoDoLote = 50;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RelayDaOutbox> _logger;
    private PublicadorRabbitMq? _publicador;
    private RelogioDaClinica _relogio;

    public RelayDaOutbox(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<RelayDaOutbox> logger,
        RelogioDaClinica relogioDaClinica)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
        _relogio = relogioDaClinica;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await PublicarPendentesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Falha no ciclo do relay da outbox");
            }
        }
    }

    private async Task PublicarPendentesAsync(CancellationToken ct)
    {
        var publicador = await ObterPublicadorAsync(ct);
        if (publicador is null) return; // RabbitMQ fora: tenta no próximo ciclo

        using var scope = _scopeFactory.CreateScope(); // DbContext novo a cada ciclo
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await using var transacao = await context.Database.BeginTransactionAsync(ct);

        // SQL cru por causa do FOR UPDATE SKIP LOCKED. Não coloque LINQ em cima disso.
        var pendentes = await context.OutboxMensagens
            .FromSql($"""
                SELECT * FROM "OutboxMensagens"
                WHERE "PublicadoEm" IS NULL
                ORDER BY "CriadoEm"
                LIMIT {TamanhoDoLote}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(ct);

        foreach (var mensagem in pendentes)
        {
            try
            {
                await publicador.PublicarAsync(mensagem, ct);
                mensagem.MarcarComoPublicada(_relogio.AgoraUtc());
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                mensagem.RegistrarFalha(ex.Message);
                _logger.LogWarning(ex, "Falha ao publicar {MensagemId}; o lote para aqui", mensagem.Id);
                break; // preserva a ordem: as próximas esperam o próximo ciclo
            }
        }

        await context.SaveChangesAsync(ct);
        await transacao.CommitAsync(ct);
    }

    private async Task<PublicadorRabbitMq?> ObterPublicadorAsync(CancellationToken ct)
    {
        if (_publicador is { EstaAberto: true }) return _publicador;

        await DescartarPublicadorAsync();

        try
        {
            _publicador = await PublicadorRabbitMq.CriarAsync(
                _configuration["RabbitMq:ConnectionString"]!, ct);
            return _publicador;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning("RabbitMQ indisponível, tentando no próximo ciclo: {Erro}", ex.Message);
            return null;
        }
    }

    private async Task DescartarPublicadorAsync()
    {
        if (_publicador is null) return;

        try { await _publicador.DisposeAsync(); }
        catch { /* a conexão já estava morta */ }

        _publicador = null;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await DescartarPublicadorAsync();
    }
}