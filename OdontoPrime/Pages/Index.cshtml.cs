using Microsoft.AspNetCore.Mvc.RazorPages;
using OdontoPrime.Api.Dtos.Consulta;
using OdontoPrime.Services;

namespace OdontoPrime.Pages;

public class IndexModel : PageModel
{
    private readonly IPacienteApiService _pacienteApiService;
    private readonly IProfissionalApiService _profissionalApiService;
    private readonly IConsultaApiService _consultaApiService;

    public IndexModel(
        IPacienteApiService pacienteApiService,
        IProfissionalApiService profissionalApiService,
        IConsultaApiService consultaApiService)
    {
        _pacienteApiService = pacienteApiService;
        _profissionalApiService = profissionalApiService;
        _consultaApiService = consultaApiService;
    }

    public int TotalPacientes { get; set; }
    public int TotalProfissionais { get; set; }
    public int ConsultasHoje { get; set; }
    public List<ConsultaResponseDTO> ProximasConsultas { get; set; } = [];

    public async Task OnGetAsync()
    {
        var pacientes = await _pacienteApiService.ListarAsync();
        var profissionais = await _profissionalApiService.ListarAsync();
        var consultas = await _consultaApiService.ListarAsync();

        TotalPacientes = pacientes.Count;
        TotalProfissionais = profissionais.Count;

        // DataHora vem em UTC da API; convertemos para local antes de comparar com "hoje".
        var hoje = DateTime.Now.Date;
        ConsultasHoje = consultas.Count(c => c.Status == "Agendada" && c.DataHora.ToLocalTime().Date == hoje);

        ProximasConsultas = consultas
            .Where(c => c.Status == "Agendada" && c.DataHora >= DateTime.UtcNow)
            .OrderBy(c => c.DataHora)
            .Take(5)
            .ToList();
    }
}
