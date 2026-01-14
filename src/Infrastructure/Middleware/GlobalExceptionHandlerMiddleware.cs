using System.Net;
using System.Text.Json;

namespace TaskHub.API.Infrastructure.Middleware;

public sealed partial class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await _next(context);
        }
#pragma warning disable CA1031 // Do not catch general exception types - This is a global exception handler
        catch (Exception exception)
#pragma warning restore CA1031
        {
            LogUnhandledException(_logger, context.TraceIdentifier, exception);
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, responseObject) = exception switch
        {
            ArgumentException argEx => CreateValidationProblemResponse(context, argEx),
            _ => CreateInternalServerErrorResponse(context, exception)
        };

        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(responseObject, JsonOptions));
    }

    private static (int StatusCode, object Response) CreateValidationProblemResponse(HttpContext context, ArgumentException exception)
    {
        var paramName = exception.ParamName ?? "request";

        // Map service-level parameter names to DTO property names
        var propertyName = MapParameterNameToPropertyName(paramName);

        var response = new
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred.",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = "Please refer to the errors property for additional details.",
            Instance = context.Request.Path.ToString(),
            Errors = new Dictionary<string, string[]>
            {
                { propertyName, new[] { exception.Message } }
            }
        };

        return ((int)HttpStatusCode.BadRequest, response);
    }

    private (int StatusCode, object Response) CreateInternalServerErrorResponse(HttpContext context, Exception exception)
    {
        var response = new
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "An error occurred while processing your request.",
            Status = (int)HttpStatusCode.InternalServerError,
            Instance = context.Request.Path.ToString(),
            Detail = _environment.IsDevelopment() ? exception.ToString() : "An unexpected error occurred. Please try again later.",
            TraceId = context.TraceIdentifier
        };

        return ((int)HttpStatusCode.InternalServerError, response);
    }

    private static string MapParameterNameToPropertyName(string paramName)
    {
        return char.ToUpperInvariant(paramName[0]) + paramName[1..];
    }

    private static partial class Log
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Error,
            Message = "An unhandled exception occurred while processing the request. TraceId: {TraceId}")]
        internal static partial void UnhandledException(ILogger logger, string traceId, Exception exception);
    }

    private static void LogUnhandledException(ILogger logger, string traceId, Exception exception)
    {
        Log.UnhandledException(logger, traceId, exception);
    }
}
