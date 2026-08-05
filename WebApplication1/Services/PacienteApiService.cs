using System.Net.Http.Json;
using WebApplication1.Api.Dtos.Paciente;

namespace WebApplication1.Services;

public interface IPacienteApiService
{
    Task<List<PacienteResponseDTO>> ListarAsync();
    Task<PacienteResponseDTO?> ObterAsync(Guid id);
    Task<ApiResult<PacienteResponseDTO>> CriarAsync(PacienteRequestDTO dto);
    Task<ApiResult> AtualizarAsync(Guid id, PacienteRequestDTO dto);
    Task<ApiResult> DesativarAsync(Guid id);
    Task<ApiResult> ReativarAsync(Guid id);
}

public class PacienteApiService : IPacienteApiService
{
    private readonly HttpClient _client;

    public PacienteApiService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("Api");
    }

    public async Task<List<PacienteResponseDTO>> ListarAsync()
    {
        return await _client.GetFromJsonAsync<List<PacienteResponseDTO>>("Paciente") ?? [];
    }

    public async Task<PacienteResponseDTO?> ObterAsync(Guid id)
    {
        var response = await _client.GetAsync($"Paciente/{id}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<PacienteResponseDTO>();
    }

    public async Task<ApiResult<PacienteResponseDTO>> CriarAsync(PacienteRequestDTO dto)
    {
        var response = await _client.PostAsJsonAsync("Paciente", dto);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<PacienteResponseDTO>.Fail(await ApiErrorHelper.ExtractErrorMessageAsync(response));
        }

        var criado = await response.Content.ReadFromJsonAsync<PacienteResponseDTO>();
        return ApiResult<PacienteResponseDTO>.Ok(criado!);
    }

    public async Task<ApiResult> AtualizarAsync(Guid id, PacienteRequestDTO dto)
    {
        var response = await _client.PutAsJsonAsync($"Paciente/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult.Fail(await ApiErrorHelper.ExtractErrorMessageAsync(response));
        }

        return ApiResult.Ok();
    }

    public async Task<ApiResult> DesativarAsync(Guid id)
    {
        var response = await _client.DeleteAsync($"Paciente/{id}");
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult.Fail(await ApiErrorHelper.ExtractErrorMessageAsync(response));
        }

        return ApiResult.Ok();
    }

    public async Task<ApiResult> ReativarAsync(Guid id)
    {
        var response = await _client.PostAsync($"Paciente/{id}/reativar", null);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult.Fail(await ApiErrorHelper.ExtractErrorMessageAsync(response));
        }

        return ApiResult.Ok();
    }
}
