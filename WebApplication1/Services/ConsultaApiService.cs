using System.Net.Http.Json;
using WebApplication1.Api.Dtos.Consulta;

namespace WebApplication1.Services;

public interface IConsultaApiService
{
    Task<List<ConsultaResponseDTO>> ListarAsync();
    Task<ConsultaResponseDTO?> ObterAsync(Guid id);
    Task<ApiResult<ConsultaResponseDTO>> CriarAsync(ConsultaRequestDTO dto);
    Task<ApiResult> ReagendarAsync(Guid id, DateTime novaDataHora);
    Task<ApiResult> CancelarAsync(Guid id);
    Task<ApiResult> RealizarAsync(Guid id);
}

public class ConsultaApiService : IConsultaApiService
{
    private readonly HttpClient _client;

    public ConsultaApiService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("Api");
    }

    public async Task<List<ConsultaResponseDTO>> ListarAsync()
    {
        return await _client.GetFromJsonAsync<List<ConsultaResponseDTO>>("Consulta") ?? [];
    }

    public async Task<ConsultaResponseDTO?> ObterAsync(Guid id)
    {
        var response = await _client.GetAsync($"Consulta/{id}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ConsultaResponseDTO>();
    }

    public async Task<ApiResult<ConsultaResponseDTO>> CriarAsync(ConsultaRequestDTO dto)
    {
        dto = dto with { DataHora = ParaUtc(dto.DataHora) };

        var response = await _client.PostAsJsonAsync("Consulta", dto);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<ConsultaResponseDTO>.Fail(await ApiErrorHelper.ExtractErrorMessageAsync(response));
        }

        var criada = await response.Content.ReadFromJsonAsync<ConsultaResponseDTO>();
        return ApiResult<ConsultaResponseDTO>.Ok(criada!);
    }

    public async Task<ApiResult> ReagendarAsync(Guid id, DateTime novaDataHora)
    {
        var response = await _client.PutAsJsonAsync($"Consulta/{id}/reagendar", ParaUtc(novaDataHora));
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult.Fail(await ApiErrorHelper.ExtractErrorMessageAsync(response));
        }

        return ApiResult.Ok();
    }

    // O input datetime-local do navegador chega sem fuso (Kind=Unspecified); o Postgres exige UTC
    // para colunas timestamptz, então tratamos esse valor como horário local do servidor e convertemos.
    private static DateTime ParaUtc(DateTime dataHora) => dataHora.Kind switch
    {
        DateTimeKind.Utc => dataHora,
        DateTimeKind.Local => dataHora.ToUniversalTime(),
        _ => DateTime.SpecifyKind(dataHora, DateTimeKind.Local).ToUniversalTime()
    };

    public async Task<ApiResult> CancelarAsync(Guid id)
    {
        var response = await _client.PostAsync($"Consulta/{id}/cancelar", null);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult.Fail(await ApiErrorHelper.ExtractErrorMessageAsync(response));
        }

        return ApiResult.Ok();
    }

    public async Task<ApiResult> RealizarAsync(Guid id)
    {
        var response = await _client.PostAsync($"Consulta/{id}/realizar", null);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult.Fail(await ApiErrorHelper.ExtractErrorMessageAsync(response));
        }

        return ApiResult.Ok();
    }
}
