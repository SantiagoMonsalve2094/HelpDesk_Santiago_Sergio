using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HelpDesk.Backend.Application.DTOs.Auth;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Api.Tests.Common;

internal static class ApiTestClient
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    internal static async Task<string> LoginAsync(
        this HttpClient client,
        string email = HelpDeskApiFactory.AdminEmail,
        string password = HelpDeskApiFactory.AdminPassword)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password });
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        return login!.AccessToken;
    }

    internal static void UseBearer(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    internal static async Task<Guid> CreateCategoryAsync(
        this HttpClient client,
        string name = "Soporte de hardware",
        string description = "Atención de equipos de cómputo y periféricos.",
        int lowSlaMinutes = 1440,
        int mediumSlaMinutes = 480,
        int highSlaMinutes = 240,
        int criticalSlaMinutes = 120)
    {
        var response = await client.PostAsJsonAsync(
            "/api/support-categories",
            new
            {
                name,
                description,
                lowSlaMinutes,
                mediumSlaMinutes,
                highSlaMinutes,
                criticalSlaMinutes
            });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Guid>())!;
    }

    internal static async Task<Guid> CreateUserAsync(
        this HttpClient client,
        string email,
        string password,
        UserRole role,
        Guid? categoryId = null,
        string? fullName = null,
        int technicianCapacity = 3)
    {
        var categories = categoryId.HasValue ? new[] { categoryId.Value } : [];
        fullName ??= role switch
        {
            UserRole.User => "Santiago Monsalve",
            UserRole.Technician => "Juan Reyes",
            UserRole.Supervisor => "Sergio Otalvaro",
            UserRole.SuperAdmin => "Santiago Monsalve",
            _ => "Laura Gómez"
        };
        var response = await client.PostAsJsonAsync(
            "/api/users",
            new
            {
                fullName,
                email,
                password,
                role = role.ToString(),
                supportCategoryIds = categories,
                maxActiveTickets = role == UserRole.Technician
                    ? technicianCapacity
                    : (int?)null
            });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Guid>())!;
    }

    internal static async Task<CreatedTicketResponse> CreateTicketAsync(
        this HttpClient client,
        Guid supportCategoryId,
        string subject = "Falla en equipo de cómputo",
        string description = "Este es un ticket de hardware.",
        string priority = "high")
    {
        var response = await client.PostAsJsonAsync(
            "/api/tickets",
            new
            {
                subject,
                description,
                supportCategoryId,
                priority
            });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreatedTicketResponse>(JsonOptions))!;
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
        return options;
    }
}
