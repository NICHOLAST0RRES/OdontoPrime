using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Api.Dtos.Medico;
using WebApplication1.Data;
using WebApplication1.Domain.Models;

namespace WebApplication1.Api.Controllers;


[ApiController]
[Route("[controller]")]
public class MedicoController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    
    
    public MedicoController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MedicoResponseDTO>>> ListarMedicos()
    {
        var medicos = await _context.Medicos
            .Include(m => m.Especialidade)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<MedicoResponseDTO>>(medicos));
    }
    
    
    [HttpGet ("{id}")]
    public async Task<ActionResult<MedicoResponseDTO>> BuscarMedico(int id)
    {
        // transformar um dto em um entidade atraves da interface Imapper
        var medico = await _context.Medicos
                
                // entender esse incluide 
            .Include(m => m.Especialidade)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        
        if (medico is null) return NotFound();
        
        return Ok(_mapper.Map<MedicoResponseDTO>(medico));
    }
    
    [HttpPost]
    public async Task<ActionResult<MedicoRequestDTO>> CadastrarMedico(MedicoRequestDTO requestDto)
    {
        // transformar um dto em um entidade atraves da interface Imapper
        var medico = _mapper.Map<Medico>(requestDto);
        _context.Medicos.Add(medico); 
        await _context.SaveChangesAsync();
        
        
        var response = _mapper.Map<MedicoResponseDTO>(medico);
        return CreatedAtAction(nameof(BuscarMedico), new { id = medico.Id }, response);
        
        
    }
    
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DesativarMedico(int id)
    {
        var medico = await _context.Medicos.FindAsync(id);
        if (medico is null)
        {
            return NotFound();
        }

        _context.Medicos.Remove(medico); // vira UPDATE via interceptor
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/reativar")]
    public async Task<IActionResult> ReativarMedico(int id)
    {
        var medico = await _context.Medicos
            .IgnoreQueryFilters()  // sem isso, a query nem acha o desativado
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medico is null)
        {
            return NotFound();
        }

        medico.Reativar();
        await _context.SaveChangesAsync();
        return NoContent();
    }
    


}