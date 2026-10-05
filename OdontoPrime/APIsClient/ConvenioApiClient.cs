using System.Net.Http.Json;
using OdontoPrime.Api.Dtos.Convenio;

namespace OdontoPrime.Services;

public interface IConvenioApiService
{
    Task<List<ConvenioResponseDTO>> ListarAsync();
}

public class ConvenioApiClient : IConvenioApiService
{
    private readonly HttpClient _client;

    public ConvenioApiClient(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("Api");
    }

    public async Task<List<ConvenioResponseDTO>> ListarAsync()
    {
        return await _client.GetFromJsonAsync<List<ConvenioResponseDTO>>("Convenio") ?? [];
    }
}
