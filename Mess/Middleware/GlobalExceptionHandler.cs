using MESS.Application.Common.Behaviors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MESS.Mess.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException ve)
        {
            httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Type = "https://tools.ietf.org/html/rfc4918#section-11.2"
            };
            problemDetails.Extensions["errors"] = ve.Errors;
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

            _logger.LogWarning("Validation error. TraceId: {TraceId}", httpContext.TraceIdentifier);

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var serverError = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        };
        serverError.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (_env.IsDevelopment())
        {
            serverError.Detail = exception.Message;
            serverError.Extensions["stackTrace"] = exception.StackTrace;
        }
        else
        {
            serverError.Detail = "An unexpected error occurred. Please contact support with the TraceId.";
        }

        await httpContext.Response.WriteAsJsonAsync(serverError, cancellationToken);
        return true;
    }
}
