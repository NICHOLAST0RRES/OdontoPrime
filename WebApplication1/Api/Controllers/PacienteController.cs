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
    public readonly AppDbContext _context;
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
    public async Task<ActionResult<PacienteResponseDTO>> BuscarPaciente(int id)
    {
        var paciente = await _context.Pacientes
                
            .Include(p => p.Convenio)
            .FirstOrDefaultAsync(p => p.Id == id);
        
        
        if (paciente is null) return NotFound();
        
        return Ok(_mapper.Map<PacienteResponseDTO>(paciente));
    }
    
    
    [HttpPost]
    public async Task<ActionResult<PacienteRequestDTO>> CadastrarPaciente(PacienteRequestDTO requestDto)
    {
        // transformar um dto em um entidade atraves da interface Imapper
        var paciente = _mapper.Map<Paciente>(requestDto);
        _context.Pacientes.Add(paciente); 
        await _context.SaveChangesAsync();
        
        var response = _mapper.Map<PacienteResponseDTO>(paciente);
        return CreatedAtAction(nameof(BuscarPaciente), new { id = paciente.Id }, response);
        
        
    }


  [HttpDelete("{id}")]
    public async Task<IActionResult> DesativarPaciente(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente is null)
        {
            return NotFound();
        }

        _context.Pacientes.Remove(paciente); // vira UPDATE via interceptor
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/reativar")]
    public async Task<IActionResult> ReativarPaciente(int id)
    {
        var paciente = await _context.Pacientes
            .IgnoreQueryFilters()  // sem isso, a query nem acha o desativado
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