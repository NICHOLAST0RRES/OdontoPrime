using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Api.Dtos.TipoProfissional;
using WebApplication1.Data;

namespace WebApplication1.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TipoProfissionalController : ControllerBase
{
    private readonly AppDbContext _context;

    public TipoProfissionalController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var tipos = await _context.TipoProfissionais
            .AsNoTracking()
            .OrderBy(t => t.Nome)
            .Select(t => new TipoProfissionalResponseDTO { Id = t.Id, Nome = t.Nome })
            .ToListAsync();

        return Ok(tipos);
    }
}
