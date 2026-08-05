using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Api.Dtos.Paciente;
using WebApplication1.Services;

namespace WebApplication1.Pages.Pacientes;

public class CreateModel : PageModel
{
    private readonly IPacienteApiService _pacienteApiService;
    private readonly IConvenioApiService _convenioApiService;

    public CreateModel(IPacienteApiService pacienteApiService, IConvenioApiService convenioApiService)
    {
        _pacienteApiService = pacienteApiService;
        _convenioApiService = convenioApiService;
    }

    [BindProperty]
    public PacienteFormInput Input { get; set; } = new();

    public List<SelectListItem> Convenios { get; set; } = [];

    public async Task OnGetAsync()
    {
        await CarregarConveniosAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CarregarConveniosAsync();
            return Page();
        }

        var dto = new PacienteRequestDTO
        {
            Nome = Input.Nome,
            Cpf = Input.Cpf,
            Telefone = Input.Telefone,
            Email = Input.Email,
            ConvenioId = Input.ConvenioId
        };

        var resultado = await _pacienteApiService.CriarAsync(dto);
        if (!resultado.Success)
        {
            ModelState.AddModelError(string.Empty, resultado.ErrorMessage!);
            await CarregarConveniosAsync();
            return Page();
        }

        TempData["Mensagem"] = "Paciente cadastrado com sucesso.";
        return RedirectToPage("Index");
    }

    private async Task CarregarConveniosAsync()
    {
        var convenios = await _convenioApiService.ListarAsync();
        Convenios = convenios.Select(c => new SelectListItem(c.Nome, c.Id.ToString())).ToList();
    }
}
