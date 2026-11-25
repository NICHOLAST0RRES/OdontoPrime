using System.Net.Http.Json;
using OdontoPrime.Api.Dtos.Bloqueio;
using OdontoPrime.Application;

namespace OdontoPrime.Services;

public interface IBloqueioApiService
{
    Task<List<BloqueioResponseDTO>> ListarFuturosAsync(Guid profissionalId);
    Task<ApiResult> CriarAsync(BloqueioRequestDTO dto);
    Task<ApiResult> RemoverAsync(Guid id);
}

// A tela trabalha no horário da clínica e a API em UTC. A conversão fica aqui, como no ConsultaApiClient.
public class BloqueioApiClient : IBloqueioApiService
{
    private readonly HttpClient _client;
    private readonly RelogioDaClinica _relogio;

    public BloqueioApiClient(IHttpClientFactory httpClientFactory, RelogioDaClinica relogio)
    {
        _client = httpClientFactory.CreateClient("Api");
        _relogio = relogio;
    }

    public async Task<List<BloqueioResponseDTO>> ListarFuturosAsync(Guid profissionalId)
    {
        return await _client.GetFromJsonAsync<List<BloqueioResponseDTO>>($"Bloqueio?profissionalId={profissionalId}") ?? [];
    }

    public async Task<ApiResult> CriarAsync(BloqueioRequestDTO dto)
    {
        dto = dto with { Inicio = _relogio.ParaUtc(dto.Inicio), Fim = _relogio.ParaUtc(dto.Fim) };

        var response = await _client.PostAsJsonAsync("Bloqueio", dto);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult.Fail(await ApiErrorHelper.ExtractErrorMessageAsync(response));
        }

        return ApiResult.Ok();
    }

    public async Task<ApiResult> RemoverAsync(Guid id)
    {
        var response = await _client.DeleteAsync($"Bloqueio/{id}");
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult.Fail(await ApiErrorHelper.ExtractErrorMessageAsync(response));
        }

        return ApiResult.Ok();
    }
}
