using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FE.API.Middleware;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = MapException(exception);

        LogException(exception, statusCode, httpContext);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode < 500
                ? exception.Message
                : "An unexpected error occurred. Use the traceId for support inquiries.",
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        ArgumentException          => (StatusCodes.Status400BadRequest, "Bad Request"),
        KeyNotFoundException       => (StatusCodes.Status404NotFound, "Not Found"),
        OperationCanceledException => (499, "Client Closed Request"),
        _                          => (StatusCodes.Status500InternalServerError, "Internal Server Error")
    };

    private void LogException(Exception exception, int statusCode, HttpContext httpContext)
    {
        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path;

        if (exception is OperationCanceledException)
        {
            logger.LogInformation("Request cancelled by client: {Method} {Path}", method, path);
            return;
        }

        if (statusCode < 500)
        {
            logger.LogWarning(exception, "Client error on {Method} {Path}: {Message}",
                method, path, exception.Message);
            return;
        }

        logger.LogError(exception, "Unhandled exception on {Method} {Path}", method, path);
    }
}
