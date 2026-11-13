using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OdontoPrime.Api.Dtos.TipoAtendimento;
using OdontoPrime.Data;

namespace OdontoPrime.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TipoAtendimentoController : ControllerBase
{
    private readonly AppDbContext _context;

    public TipoAtendimentoController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var tipos = await _context.TiposAtendimento
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .Select(t => new TipoAtendimentoResponseDTO
            {
                Id = t.Id,
                Nome = t.Nome,
                DuracaoPadraoMinutos = t.DuracaoPadraoMinutos,
                DuracaoMaximaMinutos = t.DuracaoMaximaMinutos
            })
            .ToListAsync();

        return Ok(tipos);
    }
}
