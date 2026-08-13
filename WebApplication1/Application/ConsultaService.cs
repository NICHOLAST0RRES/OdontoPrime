using Clinica.Contratos;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Models;
using WebApplication1.Infra.Mensageria;

namespace WebApplication1.Application;

public class ConsultaService
{
     private readonly AppDbContext _context;
    private readonly IPublicadorDeEventos _publicador;

    public ConsultaService(AppDbContext context, IPublicadorDeEventos publicador)
    {
        _context = context;
        _publicador = publicador;
    }

    public async Task<Result<Consulta>> AgendarAsync(
        Guid pacienteId,
        Guid profissionalId,
        DateTime dataHora,
        string? observacao)
    {
        var paciente = await _context.Pacientes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pacienteId);

        if (paciente is null)
        {
            return Result<Consulta>.Falha("Paciente não encontrado.", TipoError.Invalido);
        }

        var profissional = await _context.Profissionais
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == profissionalId);

        if (profissional is null)
        {
            return Result<Consulta>.Falha("Profissional não encontrado.", TipoError.Invalido);
        }

        if (profissional.TipoProfissionalId != TipoProfissional.DentistaId)
        {
            return Result<Consulta>.Falha("Consulta só pode ser marcada com dentista.", TipoError.Invalido);
        }

        if (await HorarioOcupadoAsync(profissionalId, dataHora, null))
        {
            return Result<Consulta>.Falha("Profissional já tem consulta nesse horário.", TipoError.Conflito);
        }

        try
        {
            var consulta = new Consulta(pacienteId, profissionalId, dataHora, observacao);

            _context.Consultas.Add(consulta);
            await _context.SaveChangesAsync();

            var evento = new ConsultaAgendada(
                consulta.Id,
                paciente.Nome,
                paciente.Telefone,
                profissional.Nome,
                consulta.DataHora
            );

            await _publicador.PublicarAsync(evento, RoutingKeys.ConsultaAgendada);

            return Result<Consulta>.Ok(consulta);
        }
        catch (ArgumentException ex)
        {
            return Result<Consulta>.Falha(ex.Message, TipoError.Invalido);
        }
    }

    private Task<bool> HorarioOcupadoAsync(Guid profissionalId, DateTime dataHora, Guid? ignorarId)
    {
        return _context.Consultas
            .AnyAsync(c =>
                c.ProfissionalId == profissionalId &&
                c.DataHora == dataHora &&
                c.StatusConsultaId == StatusConsulta.AgendadaId &&
                (ignorarId == null || c.Id != ignorarId));
    }
    
    
    public async Task<Result> CancelarAsync(Guid id)
    {
        var consulta = await _context.Consultas
            .Include(c => c.Paciente)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consulta is null)
        {
            return Result.Falha("Consulta não encontrada.", TipoError.NaoEncontrado);
        }

        try
        {
            consulta.Cancelar();
            await _context.SaveChangesAsync();

            var evento = new ConsultaCancelada(
                consulta.Id,
                consulta.Paciente.Nome,
                consulta.Paciente.Telefone,
                consulta.DataHora
            );

            await _publicador.PublicarAsync(evento, RoutingKeys.ConsultaCancelada);

            return Result.Ok();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Falha(ex.Message, TipoError.Invalido);
        }
    }
    
    public async Task<Result> ReagendarAsync(Guid id, DateTime novaDataHora)
    {
        var consulta = await _context.Consultas
            .Include(c => c.Paciente)
            .Include(c => c.Profissional)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consulta is null)
        {
            return Result.Falha("Consulta não encontrada.", TipoError.NaoEncontrado);
        }

        if (await HorarioOcupadoAsync(consulta.ProfissionalId, novaDataHora, id))
        {
            return Result.Falha("Profissional já tem consulta nesse horário.", TipoError.Conflito);
        }

        var dataHoraAnterior = consulta.DataHora;

        try
        {
            consulta.Reagendar(novaDataHora);
            await _context.SaveChangesAsync();

            var evento = new ConsultaReagendada(
                consulta.Id,
                consulta.Paciente.Nome,
                consulta.Paciente.Telefone,
                consulta.Profissional.Nome,
                dataHoraAnterior,
                consulta.DataHora
            );

            await _publicador.PublicarAsync(evento, RoutingKeys.ConsultaReagendada);

            return Result.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Falha(ex.Message, TipoError.Invalido);
        }
    }
    
    public async Task<Result> RealizarAsync(Guid id)
    {
        var consulta = await _context.Consultas.FirstOrDefaultAsync(c => c.Id == id);

        if (consulta is null)
        {
            return Result.Falha("Consulta não encontrada.", TipoError.NaoEncontrado);
        }

        try
        {
            consulta.MarcarComoRealizada();
            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Falha(ex.Message, TipoError.Invalido);
        }
    }

    public Task<List<Consulta>> ListarAsync()
    {
        return QueryComIncludes()
            .OrderBy(c => c.DataHora)
            .ToListAsync();
    }

    public Task<Consulta?> ObterPorIdAsync(Guid id)
    {
        return QueryComIncludes()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    private IQueryable<Consulta> QueryComIncludes()
    {
        return _context.Consultas
            .Include(c => c.Paciente)
            .Include(c => c.Profissional)
            .Include(c => c.StatusConsulta)
            .AsNoTracking();
    }
    
}
