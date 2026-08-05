using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Api.Dtos.TipoProfissional;
using WebApplication1.Data;
using WebApplication1.Domain.Models;

namespace WebApplication1.Api.Controllers;


[ApiController]
[Route("[controller]")]
public class ProfissionalController: ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ProfissionalController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ProfissionalRequestDTO dto)
    {
        var tipoExiste = await _context.TipoProfissionais
            .AnyAsync(t => t.Id == dto.TipoProfissionalId);

        if (!tipoExiste)
        {
            return BadRequest("Tipo de profissional inválido.");
        }

        try
        {
            var profissional = new Profissional(
                dto.Nome,
                dto.Telefone,
                dto.TipoProfissionalId,
                dto.Cro
            );

            _context.Profissionais.Add(profissional);
            await _context.SaveChangesAsync();

            await _context.Entry(profissional)
                .Reference(p => p.TipoProfissional)
                .LoadAsync();

            var response = _mapper.Map<ProfissionalResponseDTO>(profissional);

            return CreatedAtAction(nameof(ObterPorId), new { id = profissional.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var profissionais = await _context.Profissionais
            .Include(p => p.TipoProfissional)
            .AsNoTracking()
            .ToListAsync();

        return Ok(_mapper.Map<List<ProfissionalResponseDTO>>(profissionais));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var profissional = await _context.Profissionais
            .Include(p => p.TipoProfissional)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (profissional is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<ProfissionalResponseDTO>(profissional));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] ProfissionalRequestDTO dto)
    {
        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(p => p.Id == id);

        if (profissional is null)
        {
            return NotFound();
        }

        profissional.Atualizar(dto.Nome, dto.Telefone);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        var profissional = await _context.Profissionais
            .FirstOrDefaultAsync(p => p.Id == id);

        if (profissional is null)
        {
            return NotFound();
        }

        _context.Profissionais.Remove(profissional);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    
    [HttpPost("{id}/reativar")]
    public async Task<IActionResult> Reativar(Guid id)
    {
        var profissional = await _context.Profissionais
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (profissional is null)
        {
            return NotFound();
        }

        profissional.Reativar();
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
}