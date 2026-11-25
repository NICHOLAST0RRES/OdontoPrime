using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OdontoPrime.Api.Dtos.Bloqueio;
using OdontoPrime.Services;

namespace OdontoPrime.Pages.Profissionais;

public class BloqueiosModel : PageModel
{
    private readonly IProfissionalApiService _profissionalApiService;
    private readonly IBloqueioApiService _bloqueioApiService;

    public BloqueiosModel(IProfissionalApiService profissionalApiService, IBloqueioApiService bloqueioApiService)
    {
        _profissionalApiService = profissionalApiService;
        _bloqueioApiService = bloqueioApiService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public BloqueioFormInput Input { get; set; } = new();

    [TempData]
    public string? Mensagem { get; set; }

    public string NomeDoDentista { get; private set; } = "";
    public List<BloqueioResponseDTO> Bloqueios { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        if (!await CarregarAsync())
        {
            return RedirectToPage("Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await CarregarAsync())
        {
            return RedirectToPage("Index");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // As regras (fim depois do início, não pode ter terminado, sem consulta agendada no período) são do service.
        var resultado = await _bloqueioApiService.CriarAsync(new BloqueioRequestDTO
        {
            ProfissionalId = Id,
            Inicio = Input.Inicio!.Value,
            Fim = Input.Fim!.Value,
            Motivo = Input.Motivo
        });

        if (!resultado.Success)
        {
            ModelState.AddModelError(string.Empty, resultado.ErrorMessage!);
            return Page();
        }

        Mensagem = "Bloqueio criado.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostRemoverAsync(Guid bloqueioId)
    {
        var resultado = await _bloqueioApiService.RemoverAsync(bloqueioId);
        Mensagem = resultado.Success ? "Bloqueio removido." : resultado.ErrorMessage;
        return RedirectToPage(new { id = Id });
    }

    private async Task<bool> CarregarAsync()
    {
        var profissional = await _profissionalApiService.ObterAsync(Id);
        if (profissional is null || profissional.TipoProfissional != "Dentista")
        {
            TempData["Erro"] = "Bloqueio de agenda só existe para dentista.";
            return false;
        }

        NomeDoDentista = profissional.Nome;
        Bloqueios = await _bloqueioApiService.ListarFuturosAsync(Id);
        return true;
    }
}
