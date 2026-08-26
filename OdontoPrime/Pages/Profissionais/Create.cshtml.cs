using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using OdontoPrime.Api.Dtos.TipoProfissional;
using OdontoPrime.Services;

namespace OdontoPrime.Pages.Profissionais;

public class CreateModel : PageModel
{
    private readonly IProfissionalApiService _profissionalApiService;
    private readonly ITipoProfissionalApiService _tipoProfissionalApiService;

    public CreateModel(IProfissionalApiService profissionalApiService, ITipoProfissionalApiService tipoProfissionalApiService)
    {
        _profissionalApiService = profissionalApiService;
        _tipoProfissionalApiService = tipoProfissionalApiService;
    }

    [BindProperty]
    public ProfissionalCreateInput Input { get; set; } = new();

    public List<SelectListItem> Tipos { get; set; } = [];

    public async Task OnGetAsync()
    {
        await CarregarTiposAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CarregarTiposAsync();
            return Page();
        }

        var dto = new ProfissionalRequestDTO
        {
            Nome = Input.Nome,
            Telefone = Input.Telefone,
            TipoProfissionalId = Input.TipoProfissionalId,
            Cro = string.IsNullOrWhiteSpace(Input.Cro) ? null : Input.Cro
        };

        var resultado = await _profissionalApiService.CriarAsync(dto);
        if (!resultado.Success)
        {
            ModelState.AddModelError(string.Empty, resultado.ErrorMessage!);
            await CarregarTiposAsync();
            return Page();
        }

        TempData["Mensagem"] = "Profissional cadastrado com sucesso.";
        return RedirectToPage("Index");
    }

    private async Task CarregarTiposAsync()
    {
        var tipos = await _tipoProfissionalApiService.ListarAsync();
        Tipos = tipos.Select(t => new SelectListItem(t.Nome, t.Id.ToString())).ToList();
    }
}
