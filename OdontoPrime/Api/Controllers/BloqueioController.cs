using Microsoft.AspNetCore.Mvc;
using OdontoPrime.Api.Dtos.Bloqueio;
using OdontoPrime.Application;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Api.Controllers;

// Só traduz HTTP: as regras (só dentista, período válido, sem consulta agendada no período) estão no BloqueioService.
[ApiController]
[Route("[controller]")]
public class BloqueioController : ControllerBase
{
    private readonly BloqueioService _service;

    public BloqueioController(BloqueioService service)
    {
        _service = service;
    }

    // GET /Bloqueio?profissionalId=...  → bloqueios que ainda não terminaram
    [HttpGet]
    public async Task<IActionResult> ListarFuturos([FromQuery] Guid profissionalId)
    {
        var bloqueios = await _service.ListarFuturosAsync(profissionalId);
        return Ok(bloqueios.Select(ParaDto).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] BloqueioRequestDTO dto)
    {
        var resultado = await _service.CriarAsync(dto.ProfissionalId, dto.Inicio, dto.Fim, dto.Motivo);
        if (!resultado.Sucesso)
        {
            return TraduzirErro(resultado);
        }

        return StatusCode(StatusCodes.Status201Created, ParaDto(resultado.Valor!));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        var resultado = await _service.RemoverAsync(id);
        if (!resultado.Sucesso)
        {
            return TraduzirErro(resultado);
        }

        return NoContent();
    }

    private static BloqueioResponseDTO ParaDto(Bloqueio bloqueio) => new()
    {
        Id = bloqueio.Id,
        ProfissionalId = bloqueio.ProfissionalId,
        Inicio = bloqueio.Inicio,
        Fim = bloqueio.Fim,
        Motivo = bloqueio.Motivo
    };

    private IActionResult TraduzirErro(Result resultado)
    {
        return resultado.TipoErro switch
        {
            TipoError.NaoEncontrado => NotFound(resultado.Erro),
            TipoError.Conflito => Conflict(resultado.Erro),
            _ => BadRequest(resultado.Erro)
        };
    }
}
