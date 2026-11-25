using Clinica.Contratos;
using Microsoft.EntityFrameworkCore;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Application;

public class ConsultaService
{
    private readonly IOutbox _outbox;
    private readonly IClinicaDbContext _context;
    private readonly RelogioDaClinica _relogio;
    private readonly PoliticaDeLembrete _politicaDeLembrete;

    private DateTimeOffset NaClinica(DateTime utc) => _relogio.ParaOffsetDaClinica(utc);

    public ConsultaService(IClinicaDbContext context, IOutbox outbox, RelogioDaClinica relogioDaClinica, PoliticaDeLembrete politica)
    {
        _context = context;
        _outbox = outbox;
        _relogio = relogioDaClinica;
        _politicaDeLembrete = politica;
    }

    public async Task<Result<Consulta>> AgendarAsync(
        Guid pacienteId,
        Guid profissionalId,
        DateTime dataHora,
        int tipoAtendimentoId,
        int? duracaoMinutos,
        string? observacao,
        string? motivoEncaixe = null)
    
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
        
        var tipo = await _context.TiposAtendimento
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tipoAtendimentoId);

        if (tipo is null)
        {
            return Result<Consulta>.Falha("Tipo de atendimento não encontrado.", TipoError.Invalido);
        }
        
        try
        {
            var agora = _relogio.AgoraUtc();
            var lembrarEm = _politicaDeLembrete.CalcularLembrarEm(dataHora, agora);
            
            var consulta = new Consulta(pacienteId, profissionalId, tipo, duracaoMinutos, dataHora, observacao, agora, lembrarEm);

            var falhaDaAgenda = await AplicarRegrasDaAgendaAsync(profissional, consulta, motivoEncaixe);
            if (falhaDaAgenda is not null)
            {
                return Result<Consulta>.Falha(falhaDaAgenda.Value.Mensagem, falhaDaAgenda.Value.Tipo);
            }
            
            if (await HorarioOcupadoAsync(profissionalId, consulta.DataHora, consulta.Fim, null))
            {
                return Result<Consulta>.Falha("Profissional já tem consulta nesse horário.", TipoError.Conflito);
            }

            _context.Consultas.Add(consulta);
            
            _outbox.Adicionar(new ConsultaAgendada(
                consulta.Id,
                paciente.Nome,
                paciente.Telefone,
                profissional.Nome,
                NaClinica(consulta.DataHora),
                NaClinica(agora)
            ), RoutingKeys.ConsultaAgendada);

            await _context.SaveChangesAsync(); // consulta + evento na mesma transação

            return Result<Consulta>.Ok(consulta);
        }
        catch (ArgumentException ex)
        {
            return Result<Consulta>.Falha(ex.Message, TipoError.Invalido);
        }
        
        catch (HorarioOcupadoException ex)
        {
            return Result<Consulta>.Falha(ex.Message, TipoError.Conflito);
        }
    }

    
    // Aplica a jornada, os bloqueios e o encaixe. Devolve a falha, ou null se o horário pode ser usado.
    private async Task<(TipoError Tipo, string Mensagem)?> AplicarRegrasDaAgendaAsync(
        Profissional profissional, Consulta consulta, string? motivoEncaixe)
    {
        // A jornada que vale é a do dia na clínica, não a do dia em UTC.
        var dataNaClinica = DateOnly.FromDateTime(_relogio.ParaHorarioDaClinica(consulta.DataHora));
        var jornada = profissional.JornadaEm(dataNaClinica, _relogio.Fuso);

        var bloqueios = (await _context.Bloqueios
                .AsNoTracking()
                .Where(b => b.ProfissionalId == profissional.Id && b.Inicio < consulta.Fim && b.Fim > consulta.DataHora)
                .ToListAsync())
            .Select(b => b.Periodo)   // em memória: o EF não traduz o construtor do Intervalo pra SQL
            .ToList();

        switch (RegraDaAgenda.Avaliar(consulta.Periodo, jornada, bloqueios))
        {
            case SituacaoDoHorario.Bloqueado:
                return (TipoError.Conflito, "A agenda do dentista está bloqueada nesse período.");

            case SituacaoDoHorario.ForaDaJornada when string.IsNullOrWhiteSpace(motivoEncaixe):
                return (TipoError.ForaDaJornada,
                    $"Fora da jornada de {profissional.Nome}. Para marcar mesmo assim, confirme como encaixe e informe o motivo.");

            case SituacaoDoHorario.ForaDaJornada:
                consulta.MarcarComoEncaixe(motivoEncaixe!);
                return null;

            default:
                return null;
        }
    }

    private Task<bool> HorarioOcupadoAsync(Guid profissionalId, DateTime inicio, DateTime fim, Guid? ignorarId)
    {
        return _context.Consultas
            .AnyAsync(c =>
                c.ProfissionalId == profissionalId &&
                c.StatusConsultaId == StatusConsulta.AgendadaId &&
                c.DataHora < fim &&
                c.Fim > inicio &&
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

            var agora = _relogio.AgoraUtc();

            _outbox.Adicionar(new ConsultaCancelada(
                consulta.Id,
                consulta.Paciente.Nome,
                consulta.Paciente.Telefone,
                NaClinica(consulta.DataHora),
                NaClinica(agora)
            ), RoutingKeys.ConsultaCancelada);

            await _context.SaveChangesAsync();

            return Result.Ok();
        }
        
        catch (InvalidOperationException ex)
        {
            return Result.Falha(ex.Message, TipoError.Invalido);
        }
    }
    
    public async Task<Result> ReagendarAsync(Guid id, DateTime novaDataHora, string? motivoEncaixe = null)
    {
        var consulta = await _context.Consultas
            .Include(c => c.Paciente)
            .Include(c => c.Profissional)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consulta is null)
        {
            return Result.Falha("Consulta não encontrada.", TipoError.NaoEncontrado);
        }

        if (await HorarioOcupadoAsync(consulta.ProfissionalId, novaDataHora, novaDataHora + consulta.Duracao, id))        {
            return Result.Falha("Profissional já tem consulta nesse horário.", TipoError.Conflito);
        }

        var dataHoraAnterior = NaClinica(consulta.DataHora);

        try
        {
            var agora = _relogio.AgoraUtc();
            var lembrarEm = _politicaDeLembrete.CalcularLembrarEm(novaDataHora, agora);
            
            consulta.Reagendar(novaDataHora , agora , lembrarEm);

            var falhaDaAgenda = await AplicarRegrasDaAgendaAsync(consulta.Profissional, consulta, motivoEncaixe);
            if (falhaDaAgenda is not null)
            {
                return Result.Falha(falhaDaAgenda.Value.Mensagem, falhaDaAgenda.Value.Tipo);
            }

            _outbox.Adicionar(new ConsultaReagendada(
                consulta.Id,
                consulta.Paciente.Nome,
                consulta.Paciente.Telefone,
                consulta.Profissional.Nome,
                dataHoraAnterior,
                NaClinica(consulta.DataHora),
                NaClinica(agora)
            ), RoutingKeys.ConsultaReagendada);

            await _context.SaveChangesAsync();

            return Result.Ok();
            
            
        }
        catch (HorarioOcupadoException ex)
        {
            return Result.Falha(ex.Message, TipoError.Conflito);
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
            .Include(c => c.TipoAtendimento)
            .AsNoTracking();
    }
    
}
