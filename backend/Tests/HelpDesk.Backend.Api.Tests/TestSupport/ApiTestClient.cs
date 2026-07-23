using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HelpDesk.Backend.Application.Features.Auth.Models;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Api.Tests.TestSupport;

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

    internal static async Task<Guid> CreateCategoryAsync(this HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/support-categories",
            new
            {
                name = $"Categoría {Guid.NewGuid():N}",
                description = "Categoría creada por una prueba de integración.",
                lowSlaMinutes = 1440,
                mediumSlaMinutes = 480,
                highSlaMinutes = 240,
                criticalSlaMinutes = 120
            });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Guid>())!;
    }

    internal static async Task CreateUserAsync(
        this HttpClient client,
        string email,
        string password,
        UserRole role,
        Guid? categoryId = null)
    {
        var categories = categoryId.HasValue ? new[] { categoryId.Value } : [];
        var response = await client.PostAsJsonAsync(
            "/api/users",
            new
            {
                fullName = $"Usuario {role}",
                email,
                password,
                role = role.ToString(),
                supportCategoryIds = categories,
                maxActiveTickets = role == UserRole.Technician ? 3 : (int?)null
            });
        response.EnsureSuccessStatusCode();
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
        return options;
    }
}
