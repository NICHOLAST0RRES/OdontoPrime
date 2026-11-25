using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OdontoPrime.Api.Dtos.Jornada;
using OdontoPrime.Domain.Models;
using OdontoPrime.Services;

namespace OdontoPrime.Pages.Profissionais;

public class JornadaModel : PageModel
{
    // A semana na ordem da clínica: segunda primeiro, domingo por último.
    public static readonly DayOfWeek[] DiasDaSemana =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    ];

    private readonly IProfissionalApiService _profissionalApiService;

    public JornadaModel(IProfissionalApiService profissionalApiService)
    {
        _profissionalApiService = profissionalApiService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public List<DiaDaJornadaInput> Dias { get; set; } = [];

    public string NomeDoDentista { get; private set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        if (!await CarregarDentistaAsync())
        {
            return RedirectToPage("Index");
        }

        var jornada = await _profissionalApiService.ObterJornadaAsync(Id);

        // A tela mostra até dois períodos por dia, em ordem de início.
        Dias = DiasDaSemana.Select(dia =>
        {
            var periodos = jornada.Where(p => p.Dia == dia).OrderBy(p => p.Inicio).ToList();
            return new DiaDaJornadaInput
            {
                Dia = dia,
                Inicio1 = periodos.ElementAtOrDefault(0)?.Inicio,
                Fim1 = periodos.ElementAtOrDefault(0)?.Fim,
                Inicio2 = periodos.ElementAtOrDefault(1)?.Inicio,
                Fim2 = periodos.ElementAtOrDefault(1)?.Fim
            };
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await CarregarDentistaAsync())
        {
            return RedirectToPage("Index");
        }

        var periodos = new List<PeriodoDeTrabalhoDTO>();
        foreach (var dia in Dias)
        {
            AdicionarPeriodo(periodos, dia.Dia, dia.Inicio1, dia.Fim1, numero: 1);
            AdicionarPeriodo(periodos, dia.Dia, dia.Inicio2, dia.Fim2, numero: 2);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // As regras (fim depois do início, passo de 15 min, sem sobreposição) são do domínio.
        var resultado = await _profissionalApiService.DefinirJornadaAsync(Id, periodos);
        if (!resultado.Success)
        {
            ModelState.AddModelError(string.Empty, resultado.ErrorMessage!);
            return Page();
        }

        TempData["Mensagem"] = $"Jornada de {NomeDoDentista} salva.";
        return RedirectToPage("Index");
    }

    private async Task<bool> CarregarDentistaAsync()
    {
        var profissional = await _profissionalApiService.ObterAsync(Id);
        if (profissional is null || profissional.TipoProfissional != "Dentista")
        {
            TempData["Erro"] = "Jornada só existe para dentista.";
            return false;
        }

        NomeDoDentista = profissional.Nome;
        return true;
    }

    // Período com início e fim entra na jornada. Em branco = não atende. Só um dos dois = erro.
    private void AdicionarPeriodo(List<PeriodoDeTrabalhoDTO> periodos, DayOfWeek dia, TimeOnly? inicio, TimeOnly? fim, int numero)
    {
        if (inicio is null && fim is null)
        {
            return;
        }

        if (inicio is null || fim is null)
        {
            ModelState.AddModelError(string.Empty, $"{PeriodoDeTrabalho.NomeDoDia(dia)}: preencha o início e o fim do {numero}º período.");
            return;
        }

        periodos.Add(new PeriodoDeTrabalhoDTO { Dia = dia, Inicio = inicio.Value, Fim = fim.Value });
    }
}
