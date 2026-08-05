using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Api.Dtos.Paciente;
using WebApplication1.Services;

namespace WebApplication1.Pages.Pacientes;

public class IndexModel : PageModel
{
    private readonly IPacienteApiService _pacienteApiService;

    public IndexModel(IPacienteApiService pacienteApiService)
    {
        _pacienteApiService = pacienteApiService;
    }

    public List<PacienteResponseDTO> Pacientes { get; set; } = [];

    [TempData]
    public string? Mensagem { get; set; }

    [TempData]
    public string? Erro { get; set; }

    public async Task OnGetAsync()
    {
        Pacientes = await _pacienteApiService.ListarAsync();
    }

    public async Task<IActionResult> OnPostDesativarAsync(Guid id)
    {
        var resultado = await _pacienteApiService.DesativarAsync(id);
        Mensagem = resultado.Success ? "Paciente desativado com sucesso." : null;
        Erro = resultado.Success ? null : resultado.ErrorMessage;

        return RedirectToPage();
    }
}
