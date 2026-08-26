using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OdontoPrime.Api.Dtos.Consulta;
using OdontoPrime.Application;
using OdontoPrime.Data;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ConsultaController : ControllerBase
{
     private readonly ConsultaService _service;
    private readonly IMapper _mapper;

    public ConsultaController(ConsultaService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Criar(ConsultaRequestDTO dto)
    {
        var resultado = await _service.AgendarAsync(
            dto.PacienteId,
            dto.ProfissionalId,
            dto.DataHora,           //controller chama a service para validar as regras de negocio.
            dto.Observacao
        );

        if (!resultado.Sucesso)
        {
            return TraduzirErro(resultado);
        }

        var criada = await _service.ObterPorIdAsync(resultado.Valor!.Id);

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Valor.Id },
            _mapper.Map<ConsultaResponseDTO>(criada));
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var consultas = await _service.ListarAsync();

        return Ok(_mapper.Map<List<ConsultaResponseDTO>>(consultas));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var consulta = await _service.ObterPorIdAsync(id);

        if (consulta is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<ConsultaResponseDTO>(consulta));
    }

    [HttpPut("{id}/reagendar")]
    public async Task<IActionResult> Reagendar(Guid id, [FromBody] DateTime novaDataHora)
    {
        var resultado = await _service.ReagendarAsync(id, novaDataHora);

        return resultado.Sucesso ? NoContent() : TraduzirErro(resultado);
    }

    [HttpPost("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        var resultado = await _service.CancelarAsync(id);

        return resultado.Sucesso ? NoContent() : TraduzirErro(resultado);
    }

    [HttpPost("{id}/realizar")]
    public async Task<IActionResult> Realizar(Guid id)
    {
        var resultado = await _service.RealizarAsync(id);

        return resultado.Sucesso ? NoContent() : TraduzirErro(resultado);
    }

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