using Microsoft.EntityFrameworkCore;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Application;

public class BloqueioService
{
     private readonly IClinicaDbContext _context;
    private readonly RelogioDaClinica _relogio;

    public BloqueioService(IClinicaDbContext context, RelogioDaClinica relogio)
    {
        _context = context;
        _relogio = relogio;
    }

    public async Task<Result<Bloqueio>> CriarAsync(Guid profissionalId, DateTime inicioUtc, DateTime fimUtc, string motivo)
    {
        var profissional = await _context.Profissionais
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == profissionalId);

        if (profissional is null)
            return Result<Bloqueio>.Falha("Profissional não encontrado.", TipoError.NaoEncontrado);

        if (profissional.TipoProfissionalId != TipoProfissional.DentistaId)
            return Result<Bloqueio>.Falha("Só dentista tem bloqueio de agenda.", TipoError.Invalido);

        Bloqueio bloqueio;
        try
        {
            bloqueio = new Bloqueio(profissionalId, inicioUtc, fimUtc, motivo, _relogio.AgoraUtc());
        }
        catch (ArgumentException ex)
        {
            return Result<Bloqueio>.Falha(ex.Message, TipoError.Invalido);
        }

        var consultasNoPeriodo = await _context.Consultas.CountAsync(c =>
            c.ProfissionalId == profissionalId &&
            c.StatusConsultaId == StatusConsulta.AgendadaId &&
            c.DataHora < bloqueio.Fim &&
            c.Fim > bloqueio.Inicio);

        if (consultasNoPeriodo > 0)
            return Result<Bloqueio>.Falha(
                $"Há {consultasNoPeriodo} consulta(s) agendada(s) nesse período. Remarque ou cancele antes de bloquear a agenda.",
                TipoError.Conflito);

        _context.Bloqueios.Add(bloqueio);
        await _context.SaveChangesAsync();

        return Result<Bloqueio>.Ok(bloqueio);
    }

    public async Task<Result> RemoverAsync(Guid id)
    {
        var bloqueio = await _context.Bloqueios.FirstOrDefaultAsync(b => b.Id == id);

        if (bloqueio is null)
            return Result.Falha("Bloqueio não encontrado.", TipoError.NaoEncontrado);

        _context.Bloqueios.Remove(bloqueio);
        await _context.SaveChangesAsync();

        return Result.Ok();
    }

    public Task<List<Bloqueio>> ListarFuturosAsync(Guid profissionalId)
    {
        var agora = _relogio.AgoraUtc();

        return _context.Bloqueios
            .AsNoTracking()
            .Where(b => b.ProfissionalId == profissionalId && b.Fim > agora)
            .OrderBy(b => b.Inicio)
            .ToListAsync();
    }
}