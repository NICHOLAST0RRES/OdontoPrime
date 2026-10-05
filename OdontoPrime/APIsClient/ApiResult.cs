namespace OdontoPrime.Services;

public class ApiResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    public static ApiResult Ok() => new() { Success = true };
    public static ApiResult Fail(string message) => new() { Success = false, ErrorMessage = message };
}

public class ApiResult<T> : ApiResult
{
    public T? Data { get; init; }

    public static ApiResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static new ApiResult<T> Fail(string message) => new() { Success = false, ErrorMessage = message };
}
