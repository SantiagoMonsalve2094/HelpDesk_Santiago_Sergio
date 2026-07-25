using System.Text.Json;

namespace HelpDesk.Backend.Api.Errors;

internal static class ApiErrorWriter
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    internal static async Task WriteAsync(
        HttpContext context,
        int status,
        string title,
        string message,
        IReadOnlyCollection<ApiErrorDetail>? errors = null)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        var response = new ApiErrorResponse(
            status,
            title,
            message,
            context.TraceIdentifier,
            errors ?? []);
        await context.Response.WriteAsJsonAsync(
            response,
            JsonOptions,
            context.RequestAborted);
    }
}
