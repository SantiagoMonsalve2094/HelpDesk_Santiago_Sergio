using System.Text.Json;
using FluentValidation;
using HelpDesk.Backend.Api.Errors;
using HelpDesk.Backend.Application.Common.Exceptions;
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
            if (mapping.Status >= StatusCodes.Status500InternalServerError)
            {
                logger.LogError(
                    exception,
                    "Unhandled request error. TraceId: {TraceId}",
                    context.TraceIdentifier);
            }
            else
            {
                logger.LogWarning(
                    "Request rejected with {Status}. TraceId: {TraceId}. Error: {Error}",
                    mapping.Status,
                    context.TraceIdentifier,
                    exception.Message);
            }

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

    private static ErrorMapping Map(Exception exception) =>
        exception switch
        {
            ValidationException validation => Validation(validation),
            InvalidCredentialsException => new(
                StatusCodes.Status401Unauthorized,
                "Credenciales inválidas",
                exception.Message),
            UnauthorizedAccessException => new(
                StatusCodes.Status403Forbidden,
                "Acceso denegado",
                exception.Message),
            KeyNotFoundException => new(
                StatusCodes.Status404NotFound,
                "Recurso no encontrado",
                exception.Message),
            DomainException domain => new(
                StatusCodes.Status409Conflict,
                "Conflicto de dominio",
                domain.Message,
                [new ApiErrorDetail(domain.Code, domain.Message, null)]),
            DbUpdateConcurrencyException => new(
                StatusCodes.Status409Conflict,
                "Conflicto de concurrencia",
                "El recurso fue modificado por otra operación. Actualice la información e inténtelo de nuevo."),
            DbUpdateException database when IsUniqueConstraint(database) => new(
                StatusCodes.Status409Conflict,
                "Conflicto de unicidad",
                "Ya existe un registro con los datos únicos indicados."),
            InvalidOperationException => new(
                StatusCodes.Status409Conflict,
                "Conflicto",
                exception.Message),
            BadHttpRequestException or JsonException or ArgumentException => new(
                StatusCodes.Status400BadRequest,
                "Solicitud inválida",
                "La solicitud contiene datos inválidos."),
            _ => new(
                StatusCodes.Status500InternalServerError,
                "Error interno",
                "Ocurrió un error inesperado.")
        };

    private static ErrorMapping Validation(ValidationException exception)
    {
        var errors = exception.Errors
            .Select(error => new ApiErrorDetail(
                string.IsNullOrWhiteSpace(error.ErrorCode) ? "VALIDATION" : error.ErrorCode,
                error.ErrorMessage,
                ToCamelCase(error.PropertyName)))
            .ToArray();
        return new ErrorMapping(
            StatusCodes.Status422UnprocessableEntity,
            "Error de validación",
            "Uno o más campos son inválidos.",
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
