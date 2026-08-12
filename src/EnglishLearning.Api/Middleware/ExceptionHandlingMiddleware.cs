using System.Net;
using System.Text.Json;
using EnglishLearning.Application.Common.Exceptions;
using EnglishLearning.Domain.Exceptions;
using FluentValidation;

namespace EnglishLearning.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
            AppException appEx => (appEx.StatusCode, appEx.Code, appEx.Message),
            DomainException domainEx => ((int)HttpStatusCode.BadRequest, domainEx.Code, domainEx.Message),
            ValidationException validationEx => (
                (int)HttpStatusCode.BadRequest,
                "VALIDATION_ERROR",
                string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage))),
            _ => ((int)HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred.")
        };

        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Request failed with {Code}: {Message}", code, message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var payload = new
        {
            success = false,
            error = new { code, message }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}
