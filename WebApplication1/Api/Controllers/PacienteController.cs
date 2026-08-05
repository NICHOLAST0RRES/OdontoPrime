using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Api.Dtos.Paciente;
using WebApplication1.Data;
using WebApplication1.Domain.Models;

namespace WebApplication1.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PacienteController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public PacienteController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PacienteResponseDTO>>> ListarPacientes()
    {
        var pacientes = await _context.Pacientes
            .Include(p => p.Convenio)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<PacienteResponseDTO>>(pacientes));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PacienteResponseDTO>> BuscarPaciente(Guid id)
    {
        var paciente = await _context.Pacientes
            .Include(p => p.Convenio)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (paciente is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<PacienteResponseDTO>(paciente));
    }

    [HttpPost]
    public async Task<ActionResult<PacienteResponseDTO>> CadastrarPaciente(PacienteRequestDTO requestDto)
    {
        var convenioExiste = await _context.Convenios
            .AnyAsync(c => c.Id == requestDto.ConvenioId);

        if (!convenioExiste)
        {
            return BadRequest("Convênio inválido.");
        }

        try
        {
            var paciente = new Paciente(
                requestDto.Nome,
                requestDto.Cpf,
                requestDto.Telefone,
                requestDto.Email,
                requestDto.ConvenioId
            );

            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            await _context.Entry(paciente)
                .Reference(p => p.Convenio)
                .LoadAsync();

            var response = _mapper.Map<PacienteResponseDTO>(paciente);

            return CreatedAtAction(nameof(BuscarPaciente), new { id = paciente.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> AtualizarPaciente(Guid id, PacienteRequestDTO requestDto)
    {
        var paciente = await _context.Pacientes
            .FirstOrDefaultAsync(p => p.Id == id);

        if (paciente is null)
        {
            return NotFound();
        }

        var convenioExiste = await _context.Convenios
            .AnyAsync(c => c.Id == requestDto.ConvenioId);

        if (!convenioExiste)
        {
            return BadRequest("Convênio inválido.");
        }

        try
        {
            paciente.Atualizar(
                requestDto.Nome,
                requestDto.Telefone,
                requestDto.Email,
                requestDto.ConvenioId
            );

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DesativarPaciente(Guid id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);

        if (paciente is null)
        {
            return NotFound();
        }

        _context.Pacientes.Remove(paciente);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/reativar")]
    public async Task<IActionResult> ReativarPaciente(Guid id)
    {
        var paciente = await _context.Pacientes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (paciente is null)
        {
            return NotFound();
        }

        paciente.Reativar();
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
}