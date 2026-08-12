namespace EnglishLearning.Application.Common.Models;

public class ApiResult<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static ApiResult<T> Fail(string code, string message) =>
        new() { Success = false, Error = new ApiError(code, message) };
}

public record ApiError(string Code, string Message);
