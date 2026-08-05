using System.Net.Http.Json;
using System.Text.Json;

namespace WebApplication1.Services;

internal static class ApiErrorHelper
{
    public static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
        {
            return response.ReasonPhrase ?? "Erro inesperado ao comunicar com a API.";
        }

        var trimmed = content.Trim();

        if (trimmed.StartsWith('"') && trimmed.EndsWith('"'))
        {
            try
            {
                return JsonSerializer.Deserialize<string>(trimmed) ?? content;
            }
            catch (JsonException)
            {
                return content;
            }
        }

        if (trimmed.StartsWith('{'))
        {
            try
            {
                var problem = JsonSerializer.Deserialize<JsonElement>(trimmed);
                if (problem.TryGetProperty("title", out var title))
                {
                    return title.GetString() ?? content;
                }
            }
            catch (JsonException)
            {
                return content;
            }
        }

        return content;
    }
}
