using System.Net;
using System.Net.Http.Json;
using OdontoPrime.Api.Dtos.Jornada;

namespace OdontoPrime.Tests;

// Função: testar os endpoints da jornada pela API HTTP de verdade (GET e PUT /Profissional/{id}/jornada).
// Protege: horários que mudam no caminho entre a tela e o banco (DayOfWeek e TimeOnly em JSON),
// e uma jornada inválida apagando a jornada anterior em vez de ser recusada.
[Collection(ColecaoDeIntegracao.Nome)]
public class JornadaApiTests(InfraDeTeste infra) : TesteDeIntegracao(infra)
{
    [Fact]
    public async Task Jornada_salva_pela_api_volta_igual()
    {
        var (_, dentistaId) = await CriarPacienteEDentistaAsync();
        var api = Infra.App.CreateClient();
        List<PeriodoDeTrabalhoDTO> enviada =
        [
            new() { Dia = DayOfWeek.Monday, Inicio = new TimeOnly(8, 0), Fim = new TimeOnly(12, 0) },
            new() { Dia = DayOfWeek.Monday, Inicio = new TimeOnly(14, 0), Fim = new TimeOnly(18, 30) }
        ];

        var resposta = await api.PutAsJsonAsync($"Profissional/{dentistaId}/jornada", enviada, Ct);
        var lida = await api.GetFromJsonAsync<List<PeriodoDeTrabalhoDTO>>($"Profissional/{dentistaId}/jornada", Ct);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Equal(enviada, lida);   // o record compara pelo conteúdo: dia, início e fim
    }

    [Fact]
    public async Task Jornada_invalida_e_recusada_e_mantem_a_anterior()
    {
        var (_, dentistaId) = await CriarPacienteEDentistaAsync();   // começa com a jornada ampla: 7 períodos
        var api = Infra.App.CreateClient();
        List<PeriodoDeTrabalhoDTO> sobreposta =
        [
            new() { Dia = DayOfWeek.Monday, Inicio = new TimeOnly(8, 0), Fim = new TimeOnly(12, 0) },
            new() { Dia = DayOfWeek.Monday, Inicio = new TimeOnly(11, 0), Fim = new TimeOnly(14, 0) }
        ];

        var resposta = await api.PutAsJsonAsync($"Profissional/{dentistaId}/jornada", sobreposta, Ct);
        var lida = await api.GetFromJsonAsync<List<PeriodoDeTrabalhoDTO>>($"Profissional/{dentistaId}/jornada", Ct);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(7, lida!.Count);
    }
}
