using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Api.Dtos.Consulta;
using WebApplication1.Services;

namespace WebApplication1.Pages.Consultas;

public class IndexModel : PageModel
{
    private readonly IConsultaApiService _consultaApiService;

    public IndexModel(IConsultaApiService consultaApiService)
    {
        _consultaApiService = consultaApiService;
    }

    public List<ConsultaResponseDTO> Consultas { get; set; } = [];

    [TempData]
    public string? Mensagem { get; set; }

    [TempData]
    public string? Erro { get; set; }

    public async Task OnGetAsync()
    {
        Consultas = await _consultaApiService.ListarAsync();
    }

    public async Task<IActionResult> OnPostCancelarAsync(Guid id)
    {
        var resultado = await _consultaApiService.CancelarAsync(id);
        Mensagem = resultado.Success ? "Consulta cancelada com sucesso." : null;
        Erro = resultado.Success ? null : resultado.ErrorMessage;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRealizarAsync(Guid id)
    {
        var resultado = await _consultaApiService.RealizarAsync(id);
        Mensagem = resultado.Success ? "Consulta marcada como realizada." : null;
        Erro = resultado.Success ? null : resultado.ErrorMessage;

        return RedirectToPage();
    }
}
