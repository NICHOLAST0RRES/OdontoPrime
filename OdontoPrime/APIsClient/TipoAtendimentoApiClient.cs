using System.Net.Http.Json;
using OdontoPrime.Api.Dtos.TipoAtendimento;

namespace OdontoPrime.Services;

public interface ITipoAtendimentoApiService
{
    Task<List<TipoAtendimentoResponseDTO>> ListarAsync();
}

public class TipoAtendimentoApiClient : ITipoAtendimentoApiService
{
    private readonly HttpClient _client;

    public TipoAtendimentoApiClient(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("Api");
    }

    public async Task<List<TipoAtendimentoResponseDTO>> ListarAsync()
    {
        return await _client.GetFromJsonAsync<List<TipoAtendimentoResponseDTO>>("TipoAtendimento") ?? [];
    }
}
