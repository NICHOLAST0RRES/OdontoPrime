using System.Net;
using System.Net.Http.Json;
using OdontoPrime.Api.Dtos.Consulta;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Tests;

// Função: testar o encaixe pela API HTTP de verdade, do jeito que as telas de nova consulta e de reagendar usam.
// Protege: a API devolvendo um código diferente de 422 para "fora da jornada" (a tela deixaria de oferecer o encaixe),
// o motivo do encaixe se perdendo entre a API e o banco, e o reagendamento ignorando o motivo enviado.
// O dentista dos testes atende todos os dias das 07:00 às 22:00. Às 22:30, está fora da jornada.
[Collection(ColecaoDeIntegracao.Nome)]
public class EncaixeApiTests(InfraDeTeste infra) : TesteDeIntegracao(infra)
{
    private static readonly DateTime ForaDaJornadaUtc = NaClinica(10, 12, 22, 30).UtcDateTime;

    [Fact]
    public async Task Fora_da_jornada_sem_motivo_devolve_422()
    {
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();
        var api = Infra.App.CreateClient();

        var resposta = await api.PostAsJsonAsync("Consulta", NovaConsulta(pacienteId, dentistaId, motivoEncaixe: null), Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task Fora_da_jornada_com_motivo_cria_encaixe()
    {
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();
        var api = Infra.App.CreateClient();

        var resposta = await api.PostAsJsonAsync("Consulta", NovaConsulta(pacienteId, dentistaId, "Dor forte"), Ct);
        var criada = await resposta.Content.ReadFromJsonAsync<ConsultaResponseDTO>(Ct);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.True(criada!.Encaixe);
        Assert.Equal("Dor forte", criada.MotivoEncaixe);
    }

    [Fact]
    public async Task Reagendar_fora_da_jornada_pede_motivo_e_aceita_com_ele()
    {
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();
        var consultaId = await AgendarAsync(pacienteId, dentistaId, NaClinica(10, 12, 10));
        var api = Infra.App.CreateClient();

        var semMotivo = await api.PutAsJsonAsync($"Consulta/{consultaId}/reagendar",
            new ReagendarConsultaDTO { NovaDataHora = ForaDaJornadaUtc }, Ct);
        var comMotivo = await api.PutAsJsonAsync($"Consulta/{consultaId}/reagendar",
            new ReagendarConsultaDTO { NovaDataHora = ForaDaJornadaUtc, MotivoEncaixe = "Único horário do paciente" }, Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, semMotivo.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, comMotivo.StatusCode);
    }

    private static ConsultaRequestDTO NovaConsulta(Guid pacienteId, Guid dentistaId, string? motivoEncaixe) => new()
    {
        PacienteId = pacienteId,
        ProfissionalId = dentistaId,
        DataHora = ForaDaJornadaUtc,
        TipoAtendimentoId = TipoAtendimento.AvaliacaoId,
        MotivoEncaixe = motivoEncaixe
    };
}
