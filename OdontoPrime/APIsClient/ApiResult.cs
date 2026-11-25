using System.Net;

namespace OdontoPrime.Services;

public class ApiResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public HttpStatusCode? StatusCode { get; init; }

    // 422: a API recusou só porque o horário está fora da jornada. A tela pode oferecer o encaixe.
    public bool ForaDaJornada => StatusCode == HttpStatusCode.UnprocessableEntity;

    public static ApiResult Ok() => new() { Success = true };

    public static ApiResult Fail(string message, HttpStatusCode? statusCode = null)
        => new() { Success = false, ErrorMessage = message, StatusCode = statusCode };
}

public class ApiResult<T> : ApiResult
{
    public T? Data { get; init; }

    public static ApiResult<T> Ok(T data) => new() { Success = true, Data = data };

    public static new ApiResult<T> Fail(string message, HttpStatusCode? statusCode = null)
        => new() { Success = false, ErrorMessage = message, StatusCode = statusCode };
}
