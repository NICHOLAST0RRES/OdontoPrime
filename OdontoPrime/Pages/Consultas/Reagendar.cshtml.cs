using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OdontoPrime.Application;
using OdontoPrime.Services;

namespace OdontoPrime.Pages.Consultas;

public class ReagendarModel : PageModel
{
    private readonly IConsultaApiService _consultaApiService;
    private readonly RelogioDaClinica _relogio;

    public ReagendarModel(IConsultaApiService consultaApiService , RelogioDaClinica relogio)
    {
        _consultaApiService = consultaApiService;
        _relogio = relogio;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Informe a nova data e hora.")]
    [DataType(DataType.DateTime)]
    public DateTime NovaDataHora { get; set; }

    [BindProperty]
    [StringLength(200, ErrorMessage = "O motivo do encaixe pode ter no máximo 200 caracteres.")]
    public string? MotivoEncaixe { get; set; }

    // Fica true quando a API respondeu "fora da jornada" (422): a tela mostra o campo de motivo do encaixe.
    public bool OferecerEncaixe { get; private set; }
    public string? AvisoEncaixe { get; private set; }

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
        NovaDataHora = _relogio.ParaHorarioDaClinica(consulta.DataHora);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CarregarContextoAsync();
            return Page();
        }

        var resultado = await _consultaApiService.ReagendarAsync(Id, NovaDataHora, MotivoEncaixe);
        if (!resultado.Success)
        {
            if (resultado.ForaDaJornada)
            {
                OferecerEncaixe = true;
                AvisoEncaixe = resultado.ErrorMessage;
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.ErrorMessage!);
                OferecerEncaixe = !string.IsNullOrWhiteSpace(MotivoEncaixe);   // mantém o campo se já estava aberto
            }

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
