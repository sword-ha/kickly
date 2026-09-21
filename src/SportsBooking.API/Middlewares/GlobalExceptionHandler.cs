using Microsoft.AspNetCore.Hosting;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.API.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment) : IExceptionHandler
{
    private  ILogger<GlobalExceptionHandler> _Logger { get; } = logger;
    private IHostEnvironment _Environment { get; } = environment;

    public async ValueTask<bool> TryHandleAsync( HttpContext httpContext, Exception exception, CancellationToken cancellationToken )
    {
        var (statusCode, title) = MapException(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _Logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _Logger.LogWarning("Handled exception ({StatusCode}): {Message}", statusCode, exception.Message);
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError && !_Environment.IsDevelopment()
                ? "An unexpected error occurred."
                : exception.Message,
            Type = $"https://tools.ietf.org/html/rfc9110#section-{TypeSectionFor(statusCode)}"
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
        ValidationDomainException => (StatusCodes.Status400BadRequest, "Bad Request"),
        ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
        ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
        PaymentFailedException => (StatusCodes.Status402PaymentRequired, "Payment Failed"),
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
    };

    private static string TypeSectionFor(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "15.5.1",
        StatusCodes.Status401Unauthorized => "15.5.2",
        StatusCodes.Status402PaymentRequired => "15.5.3",
        StatusCodes.Status403Forbidden => "15.5.4",
        StatusCodes.Status404NotFound => "15.5.5",
        StatusCodes.Status409Conflict => "15.5.10",
        _ => "15.6.1"
    };
}
