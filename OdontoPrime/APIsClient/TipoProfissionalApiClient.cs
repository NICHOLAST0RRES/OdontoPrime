using System.Net.Http.Json;
using OdontoPrime.Api.Dtos.TipoProfissional;

namespace OdontoPrime.Services;

public interface ITipoProfissionalApiService
{
    Task<List<TipoProfissionalResponseDTO>> ListarAsync();
}

public class TipoProfissionalApiClient : ITipoProfissionalApiService
{
    private readonly HttpClient _client;

    public TipoProfissionalApiClient(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("Api");
    }

    public async Task<List<TipoProfissionalResponseDTO>> ListarAsync()
    {
        return await _client.GetFromJsonAsync<List<TipoProfissionalResponseDTO>>("TipoProfissional") ?? [];
    }
}
