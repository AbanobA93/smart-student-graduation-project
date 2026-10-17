namespace SmartStudent.Application.Common;

/// <summary>
/// Standardized API response wrapper for all endpoints.
/// </summary>
public class ApiResponse<T>
{
    public bool Succeeded { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

    public static ApiResponse<T> Success(T data, string message = "Operation completed successfully.", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Succeeded = true,
            StatusCode = statusCode,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Failure(string message, int statusCode = 400, IEnumerable<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Succeeded = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors?.ToList() ?? (IReadOnlyList<string>)Array.Empty<string>()
        };
    }
}
