namespace NotificaPix.Core.Contracts.Common;

public record ApiResponse<T>(bool Success, T? Data = default, string? Error = null, string? Code = null)
{
    public static ApiResponse<T> Ok(T data) => new(true, data);
    public static ApiResponse<T> Fail(string error, string? code = null) => new(false, default, error, code);
    public static ApiResponse<T> Empty() => new(true, default);
}
