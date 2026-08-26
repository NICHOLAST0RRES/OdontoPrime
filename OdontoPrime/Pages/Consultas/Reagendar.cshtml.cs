using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OdontoPrime.Services;

namespace OdontoPrime.Pages.Consultas;

public class ReagendarModel : PageModel
{
    private readonly IConsultaApiService _consultaApiService;

    public ReagendarModel(IConsultaApiService consultaApiService)
    {
        _consultaApiService = consultaApiService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Informe a nova data e hora.")]
    [DataType(DataType.DateTime)]
    public DateTime NovaDataHora { get; set; }

    public string? PacienteNome { get; set; }
    public string? ProfissionalNome { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var consulta = await _consultaApiService.ObterAsync(Id);
        if (consulta is null || consulta.Status != "Agendada")
        {
            return NotFound();
        }

        PacienteNome = consulta.PacienteNome;
        ProfissionalNome = consulta.ProfissionalNome;
        NovaDataHora = consulta.DataHora.ToLocalTime();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CarregarContextoAsync();
            return Page();
        }

        var resultado = await _consultaApiService.ReagendarAsync(Id, NovaDataHora);
        if (!resultado.Success)
        {
            ModelState.AddModelError(string.Empty, resultado.ErrorMessage!);
            await CarregarContextoAsync();
            return Page();
        }

        TempData["Mensagem"] = "Consulta reagendada com sucesso.";
        return RedirectToPage("Index");
    }

    private async Task CarregarContextoAsync()
    {
        var consulta = await _consultaApiService.ObterAsync(Id);
        PacienteNome = consulta?.PacienteNome;
        ProfissionalNome = consulta?.ProfissionalNome;
    }
}
