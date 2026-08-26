using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using OdontoPrime.Api.Dtos.Consulta;
using OdontoPrime.Services;

namespace OdontoPrime.Pages.Consultas;

public class CreateModel : PageModel
{
    private readonly IConsultaApiService _consultaApiService;
    private readonly IPacienteApiService _pacienteApiService;
    private readonly IProfissionalApiService _profissionalApiService;

    public CreateModel(
        IConsultaApiService consultaApiService,
        IPacienteApiService pacienteApiService,
        IProfissionalApiService profissionalApiService)
    {
        _consultaApiService = consultaApiService;
        _pacienteApiService = pacienteApiService;
        _profissionalApiService = profissionalApiService;
    }

    [BindProperty]
    public ConsultaFormInput Input { get; set; } = new();

    public List<SelectListItem> Pacientes { get; set; } = [];
    public List<SelectListItem> Dentistas { get; set; } = [];

    public async Task OnGetAsync()
    {
        await CarregarListasAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CarregarListasAsync();
            return Page();
        }

        var dto = new ConsultaRequestDTO
        {
            PacienteId = Input.PacienteId,
            ProfissionalId = Input.ProfissionalId,
            DataHora = Input.DataHora,
            Observacao = string.IsNullOrWhiteSpace(Input.Observacao) ? null : Input.Observacao
        };

        var resultado = await _consultaApiService.CriarAsync(dto);
        if (!resultado.Success)
        {
            ModelState.AddModelError(string.Empty, resultado.ErrorMessage!);
            await CarregarListasAsync();
            return Page();
        }

        TempData["Mensagem"] = "Consulta agendada com sucesso.";
        return RedirectToPage("Index");
    }

    private async Task CarregarListasAsync()
    {
        var pacientes = await _pacienteApiService.ListarAsync();
        Pacientes = pacientes.Select(p => new SelectListItem($"{p.Nome} ({p.Cpf})", p.Id.ToString())).ToList();

        var profissionais = await _profissionalApiService.ListarAsync();
        Dentistas = profissionais
            .Where(p => p.TipoProfissional == "Dentista")
            .Select(p => new SelectListItem(p.Nome, p.Id.ToString()))
            .ToList();
    }
}
