using System.Net;
using System.Text.Json;
using SportsBooking.Application.DTOs;
using SportsBooking.Domain.Exceptions;

namespace SportsBooking.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        var (statusCode, code, message) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "NOT_FOUND", exception.Message),
            ConflictException => (HttpStatusCode.Conflict, "CONFLICT", exception.Message),
            EmailNotConfirmedException => (HttpStatusCode.Forbidden, "EMAIL_NOT_CONFIRMED", exception.Message),
            ForbiddenException => (HttpStatusCode.Forbidden, "FORBIDDEN", exception.Message),
            ValidationDomainException => (HttpStatusCode.BadRequest, "VALIDATION_ERROR", exception.Message),
            PaymentFailedException => (HttpStatusCode.PaymentRequired, "PAYMENT_FAILED", exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", "You are not authorized."),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var error = new ApiError(code, message);
        var json = JsonSerializer.Serialize(error);
        await context.Response.WriteAsync(json);
    }
}