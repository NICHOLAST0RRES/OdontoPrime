using System.Net.Http.Json;
using WebApplication1.Api.Dtos.TipoProfissional;

namespace WebApplication1.Services;

public interface IProfissionalApiService
{
    Task<List<ProfissionalResponseDTO>> ListarAsync();
    Task<ProfissionalResponseDTO?> ObterAsync(Guid id);
    Task<ApiResult<ProfissionalResponseDTO>> CriarAsync(ProfissionalRequestDTO dto);
    Task<ApiResult> AtualizarAsync(Guid id, ProfissionalRequestDTO dto);
    Task<ApiResult> DesativarAsync(Guid id);
    Task<ApiResult> ReativarAsync(Guid id);
}

public class ProfissionalApiService : IProfissionalApiService
{
    private readonly HttpClient _client;

    public ProfissionalApiService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("Api");
    }

    public async Task<List<ProfissionalResponseDTO>> ListarAsync()
    {
        return await _client.GetFromJsonAsync<List<ProfissionalResponseDTO>>("Profissional") ?? [];
    }

    public async Task<ProfissionalResponseDTO?> ObterAsync(Guid id)
    {
        var response = await _client.GetAsync($"Profissional/{id}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ProfissionalResponseDTO>();
    }

    public async Task<ApiResult<ProfissionalResponseDTO>> CriarAsync(ProfissionalRequestDTO dto)
    {
        var response = await _client.PostAsJsonAsync("Profissional", dto);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<ProfissionalResponseDTO>.Fail(await ApiErrorHelper.ExtractErrorMessageAsync(response));
        }

        var criado = await response.Content.ReadFromJsonAsync<ProfissionalResponseDTO>();
        return ApiResult<ProfissionalResponseDTO>.Ok(criado!);
    }

    public async Task<ApiResult> AtualizarAsync(Guid id, ProfissionalRequestDTO dto)
    {
        var response = await _client.PutAsJsonAsync($"Profissional/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult.Fail(await ApiErrorHelper.ExtractErrorMessageAsync(response));
        }

        return ApiResult.Ok();
    }

    public async Task<ApiResult> DesativarAsync(Guid id)
    {
        var response = await _client.DeleteAsync($"Profissional/{id}");
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult.Fail(await ApiErrorHelper.ExtractErrorMessageAsync(response));
        }

        return ApiResult.Ok();
    }

    public async Task<ApiResult> ReativarAsync(Guid id)
    {
        var response = await _client.PostAsync($"Profissional/{id}/reativar", null);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult.Fail(await ApiErrorHelper.ExtractErrorMessageAsync(response));
        }

        return ApiResult.Ok();
    }
}
