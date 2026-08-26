using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using OdontoPrime.Api.Dtos.Paciente;
using OdontoPrime.Services;

namespace OdontoPrime.Pages.Pacientes;

public class EditModel : PageModel
{
    private readonly IPacienteApiService _pacienteApiService;
    private readonly IConvenioApiService _convenioApiService;

    public EditModel(IPacienteApiService pacienteApiService, IConvenioApiService convenioApiService)
    {
        _pacienteApiService = pacienteApiService;
        _convenioApiService = convenioApiService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public PacienteFormInput Input { get; set; } = new();

    public List<SelectListItem> Convenios { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var paciente = await _pacienteApiService.ObterAsync(Id);
        if (paciente is null)
        {
            return NotFound();
        }

        Input = new PacienteFormInput
        {
            Nome = paciente.Nome,
            Cpf = paciente.Cpf,
            Telefone = paciente.Telefone,
            Email = paciente.Email,
            ConvenioId = paciente.ConvenioId
        };

        await CarregarConveniosAsync();
        return Page();
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

        var resultado = await _pacienteApiService.AtualizarAsync(Id, dto);
        if (!resultado.Success)
        {
            ModelState.AddModelError(string.Empty, resultado.ErrorMessage!);
            await CarregarConveniosAsync();
            return Page();
        }

        TempData["Mensagem"] = "Paciente atualizado com sucesso.";
        return RedirectToPage("Index");
    }

    private async Task CarregarConveniosAsync()
    {
        var convenios = await _convenioApiService.ListarAsync();
        Convenios = convenios.Select(c => new SelectListItem(c.Nome, c.Id.ToString())).ToList();
    }
}
