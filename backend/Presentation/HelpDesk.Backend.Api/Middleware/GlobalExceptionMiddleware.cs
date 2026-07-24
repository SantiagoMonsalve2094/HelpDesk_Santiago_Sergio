using System.Text.Json;
using FluentValidation;
using HelpDesk.Backend.Api.Errors;
using HelpDesk.Backend.Api.Resources;
using HelpDesk.Backend.Application.Exceptions;
using HelpDesk.Backend.Domain.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Backend.Api.Middleware;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug(
                "Request cancelled by the client. TraceId: {TraceId}",
                context.TraceIdentifier);
        }
        catch (Exception exception)
        {
            var mapping = Map(exception);
            LogException(logger, context.TraceIdentifier, exception, mapping.Status);

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                await ApiErrorWriter.WriteAsync(
                    context,
                    mapping.Status,
                    mapping.Title,
                    mapping.Message,
                    mapping.Errors);
            }
        }
    }

    private static void LogException(
        ILogger logger,
        string traceId,
        Exception exception,
        int status)
    {
        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled request error. TraceId: {TraceId}",
                traceId);
            return;
        }

        logger.LogWarning(
            "Request rejected with {Status}. TraceId: {TraceId}. Error: {Error}",
            status,
            traceId,
            exception.Message);
    }

    private static ErrorMapping Map(Exception exception) =>
        exception switch
        {
            ValidationException validation => Validation(validation),
            InvalidCredentialsException => new(
                StatusCodes.Status401Unauthorized,
                ApiMessages.InvalidCredentialsTitle,
                exception.Message),
            UnauthorizedAccessException => new(
                StatusCodes.Status403Forbidden,
                ApiMessages.AccessDeniedTitle,
                exception.Message),
            KeyNotFoundException => new(
                StatusCodes.Status404NotFound,
                ApiMessages.ResourceNotFoundTitle,
                exception.Message),
            DomainException domain => new(
                StatusCodes.Status409Conflict,
                ApiMessages.DomainConflictTitle,
                domain.Message,
                [new ApiErrorDetail(domain.Code, domain.Message, null)]),
            DbUpdateConcurrencyException => new(
                StatusCodes.Status409Conflict,
                ApiMessages.ConcurrencyConflictTitle,
                ApiMessages.ConcurrencyConflict),
            DbUpdateException database when IsUniqueConstraint(database) => new(
                StatusCodes.Status409Conflict,
                ApiMessages.UniquenessConflictTitle,
                ApiMessages.UniquenessConflict),
            InvalidOperationException => new(
                StatusCodes.Status409Conflict,
                ApiMessages.ConflictTitle,
                exception.Message),
            BadHttpRequestException or JsonException or ArgumentException => new(
                StatusCodes.Status400BadRequest,
                ApiMessages.InvalidRequestTitle,
                ApiMessages.InvalidRequest),
            _ => new(
                StatusCodes.Status500InternalServerError,
                ApiMessages.UnexpectedErrorTitle,
                ApiMessages.UnexpectedError)
        };

    private static ErrorMapping Validation(ValidationException exception)
    {
        var errors = exception.Errors
            .Select(error => new ApiErrorDetail(
                string.IsNullOrWhiteSpace(error.ErrorCode)
                    ? ApiErrorCodes.Validation
                    : error.ErrorCode,
                error.ErrorMessage,
                ToCamelCase(error.PropertyName)))
            .ToArray();

        return new ErrorMapping(
            StatusCodes.Status422UnprocessableEntity,
            ApiMessages.ValidationTitle,
            ApiMessages.Validation,
            errors);
    }

    private static bool IsUniqueConstraint(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };

    private static string? ToCamelCase(string? value) =>
        string.IsNullOrEmpty(value)
            ? value
            : char.ToLowerInvariant(value[0]) + value[1..];

    private sealed record ErrorMapping(
        int Status,
        string Title,
        string Message,
        IReadOnlyCollection<ApiErrorDetail>? Errors = null);
}
