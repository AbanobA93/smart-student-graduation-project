using System.Net;
using System.Text.Json;
using SmartStudent.Application.Common;
using SmartStudent.Core.Exceptions;

namespace SmartStudent.API.Middlewares;

/// <summary>
/// Global Exception Handling Middleware catching all DomainExceptions and unhandled server exceptions,
/// logging them via ILogger (Serilog) and returning a standardized ApiResponse JSON payload.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        string message;
        IEnumerable<string>? errors = null;

        if (exception is DomainException domainEx)
        {
            statusCode = domainEx.StatusCode;
            message = domainEx.Message;

            if (domainEx is BadRequestException badReqEx && badReqEx.Errors.Count > 0)
            {
                errors = badReqEx.Errors;
            }

            _logger.LogWarning("Domain exception ({StatusCode}): {Message}", statusCode, message);
        }
        else
        {
            statusCode = (int)HttpStatusCode.InternalServerError;
            message = "An unexpected server error occurred. Please try again later.";
            _logger.LogError(exception, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
        }

        context.Response.StatusCode = statusCode;
        var response = ApiResponse<object>.Failure(message, statusCode, errors);
        var json = JsonSerializer.Serialize(response, JsonOptions);

        await context.Response.WriteAsync(json);
    }
}
