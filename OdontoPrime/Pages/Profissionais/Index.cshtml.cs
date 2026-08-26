using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OdontoPrime.Api.Dtos.TipoProfissional;
using OdontoPrime.Services;

namespace OdontoPrime.Pages.Profissionais;

public class IndexModel : PageModel
{
    private readonly IProfissionalApiService _profissionalApiService;

    public IndexModel(IProfissionalApiService profissionalApiService)
    {
        _profissionalApiService = profissionalApiService;
    }

    public List<ProfissionalResponseDTO> Profissionais { get; set; } = [];

    [TempData]
    public string? Mensagem { get; set; }

    [TempData]
    public string? Erro { get; set; }

    public async Task OnGetAsync()
    {
        Profissionais = await _profissionalApiService.ListarAsync();
    }

    public async Task<IActionResult> OnPostDesativarAsync(Guid id)
    {
        var resultado = await _profissionalApiService.DesativarAsync(id);
        Mensagem = resultado.Success ? "Profissional desativado com sucesso." : null;
        Erro = resultado.Success ? null : resultado.ErrorMessage;

        return RedirectToPage();
    }
}
