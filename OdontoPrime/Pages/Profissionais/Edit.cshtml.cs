using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OdontoPrime.Api.Dtos.TipoProfissional;
using OdontoPrime.Services;

namespace OdontoPrime.Pages.Profissionais;

public class EditModel : PageModel
{
    private readonly IProfissionalApiService _profissionalApiService;

    public EditModel(IProfissionalApiService profissionalApiService)
    {
        _profissionalApiService = profissionalApiService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public ProfissionalEditInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var profissional = await _profissionalApiService.ObterAsync(Id);
        if (profissional is null)
        {
            return NotFound();
        }

        Input = new ProfissionalEditInput
        {
            Nome = profissional.Nome,
            Telefone = profissional.Telefone,
            TipoProfissional = profissional.TipoProfissional,
            Cro = profissional.Cro
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var dto = new ProfissionalRequestDTO
        {
            Nome = Input.Nome,
            Telefone = Input.Telefone
        };

        var resultado = await _profissionalApiService.AtualizarAsync(Id, dto);
        if (!resultado.Success)
        {
            ModelState.AddModelError(string.Empty, resultado.ErrorMessage!);
            return Page();
        }

        TempData["Mensagem"] = "Profissional atualizado com sucesso.";
        return RedirectToPage("Index");
    }
}
