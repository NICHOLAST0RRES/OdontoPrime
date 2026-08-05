using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Api.Dtos.Consulta;
using WebApplication1.Data;
using WebApplication1.Domain.Models;

namespace WebApplication1.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ConsultaController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ConsultaController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Criar(ConsultaRequestDTO dto)
    {
        var pacienteExiste = await _context.Pacientes
            .AnyAsync(p => p.Id == dto.PacienteId);

        if (!pacienteExiste)
        {
            return BadRequest("Paciente não encontrado.");
        }

        var profissional = await _context.Profissionais
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == dto.ProfissionalId);

        if (profissional is null)
        {
            return BadRequest("Profissional não encontrado.");
        }

        if (profissional.TipoProfissionalId != TipoProfissional.DentistaId)
        {
            return BadRequest("Consulta só pode ser marcada com dentista.");
        }

        var horarioOcupado = await _context.Consultas
            .AnyAsync(c =>
                c.ProfissionalId == dto.ProfissionalId &&
                c.DataHora == dto.DataHora &&
                c.StatusConsultaId == StatusConsulta.AgendadaId);

        if (horarioOcupado)
        {
            return Conflict("Profissional já tem consulta nesse horário.");
        }

        try
        {
            var consulta = new Consulta(
                dto.PacienteId,
                dto.ProfissionalId,
                dto.DataHora,
                dto.Observacao
            );

            _context.Consultas.Add(consulta);
            await _context.SaveChangesAsync();

            var criada = await BuscarComIncludes(consulta.Id);

            return CreatedAtAction(nameof(ObterPorId), new { id = consulta.Id },
                _mapper.Map<ConsultaResponseDTO>(criada));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var consultas = await _context.Consultas
            .Include(c => c.Paciente)
            .Include(c => c.Profissional)
            .Include(c => c.StatusConsulta)
            .OrderBy(c => c.DataHora)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<ConsultaResponseDTO>>(consultas));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var consulta = await BuscarComIncludes(id);

        if (consulta is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<ConsultaResponseDTO>(consulta));
    }

    [HttpPut("{id}/reagendar")]
    public async Task<IActionResult> Reagendar(Guid id, [FromBody] DateTime novaDataHora)
    {
        var consulta = await _context.Consultas.FirstOrDefaultAsync(c => c.Id == id);

        if (consulta is null)
        {
            return NotFound();
        }

        var horarioOcupado = await _context.Consultas
            .AnyAsync(c =>
                c.Id != id &&
                c.ProfissionalId == consulta.ProfissionalId &&
                c.DataHora == novaDataHora &&
                c.StatusConsultaId == StatusConsulta.AgendadaId);

        if (horarioOcupado)
        {
            return Conflict("Profissional já tem consulta nesse horário.");
        }

        try
        {
            consulta.Reagendar(novaDataHora);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        var consulta = await _context.Consultas.FirstOrDefaultAsync(c => c.Id == id);

        if (consulta is null)
        {
            return NotFound();
        }

        try
        {
            consulta.Cancelar();
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/realizar")]
    public async Task<IActionResult> Realizar(Guid id)
    {
        var consulta = await _context.Consultas.FirstOrDefaultAsync(c => c.Id == id);

        if (consulta is null)
        {
            return NotFound();
        }

        try
        {
            consulta.MarcarComoRealizada();
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private Task<Consulta?> BuscarComIncludes(Guid id)
    {
        return _context.Consultas
            .Include(c => c.Paciente)
            .Include(c => c.Profissional)
            .Include(c => c.StatusConsulta)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}