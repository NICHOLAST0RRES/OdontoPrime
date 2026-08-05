using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Api.Dtos.Convenio;
using WebApplication1.Data;

namespace WebApplication1.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ConvenioController : ControllerBase
{
    private readonly AppDbContext _context;

    public ConvenioController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var convenios = await _context.Convenios
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .Select(c => new ConvenioResponseDTO { Id = c.Id, Nome = c.Nome })
            .ToListAsync();

        return Ok(convenios);
    }
}
