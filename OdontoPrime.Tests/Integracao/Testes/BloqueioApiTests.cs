using System.Net;
using System.Net.Http.Json;
using OdontoPrime.Api.Dtos.Bloqueio;

namespace OdontoPrime.Tests;

// Função: testar os endpoints de bloqueio pela API HTTP de verdade (POST, GET e DELETE /Bloqueio).
// Protege: horários que mudam no caminho até o banco, um bloqueio sobre consulta agendada
// chegando à tela como erro genérico em vez de conflito (409), e um bloqueio que continua valendo depois de removido.
// Datas: o relógio dos testes está em 09/10. O bloqueio é em 20/10, das 8h às 18h na clínica.
[Collection(ColecaoDeIntegracao.Nome)]
public class BloqueioApiTests(InfraDeTeste infra) : TesteDeIntegracao(infra)
{
    private static readonly DateTime InicioUtc = NaClinica(10, 20, 8).UtcDateTime;
    private static readonly DateTime FimUtc = NaClinica(10, 20, 18).UtcDateTime;

    [Fact]
    public async Task Bloqueio_criado_pela_api_aparece_na_lista()
    {
        var (_, dentistaId) = await CriarPacienteEDentistaAsync();
        var api = Infra.App.CreateClient();

        var resposta = await api.PostAsJsonAsync("Bloqueio", NovoBloqueio(dentistaId), Ct);
        var lista = await api.GetFromJsonAsync<List<BloqueioResponseDTO>>($"Bloqueio?profissionalId={dentistaId}", Ct);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var bloqueio = Assert.Single(lista!);
        Assert.Equal(InicioUtc, bloqueio.Inicio.ToUniversalTime());
        Assert.Equal(FimUtc, bloqueio.Fim.ToUniversalTime());
        Assert.Equal("Congresso", bloqueio.Motivo);
    }

    [Fact]
    public async Task Bloqueio_sobre_consulta_agendada_devolve_409()
    {
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();
        await AgendarAsync(pacienteId, dentistaId, NaClinica(10, 20, 10));
        var api = Infra.App.CreateClient();

        var resposta = await api.PostAsJsonAsync("Bloqueio", NovoBloqueio(dentistaId), Ct);

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task Bloqueio_removido_sai_da_lista()
    {
        var (_, dentistaId) = await CriarPacienteEDentistaAsync();
        var api = Infra.App.CreateClient();
        await api.PostAsJsonAsync("Bloqueio", NovoBloqueio(dentistaId), Ct);
        var criado = (await api.GetFromJsonAsync<List<BloqueioResponseDTO>>($"Bloqueio?profissionalId={dentistaId}", Ct))!.Single();

        var resposta = await api.DeleteAsync($"Bloqueio/{criado.Id}", Ct);
        var lista = await api.GetFromJsonAsync<List<BloqueioResponseDTO>>($"Bloqueio?profissionalId={dentistaId}", Ct);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Empty(lista!);
    }

    private static BloqueioRequestDTO NovoBloqueio(Guid dentistaId) => new()
    {
        ProfissionalId = dentistaId,
        Inicio = InicioUtc,
        Fim = FimUtc,
        Motivo = "Congresso"
    };
}
