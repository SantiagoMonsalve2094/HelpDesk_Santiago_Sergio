using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using HelpDesk.Backend.Api.Tests.Common;
using HelpDesk.Backend.Domain.Enums;
using Microsoft.IdentityModel.Tokens;

namespace HelpDesk.Backend.Api.Tests;

public sealed class AuthAndSecurityTests : ApiIntegrationTestBase
{
    [Fact]
    public async Task BootstrapAdmin_CanLoginAndTokenContainsRequiredClaims()
    {
        var token = await Client.LoginAsync();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        using var payloadDocument = JsonDocument.Parse(
            Base64UrlEncoder.DecodeBytes(token.Split('.')[1]));
        var payload = payloadDocument.RootElement;

        Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.Sub);
        Assert.True(Guid.TryParse(payload.GetProperty("nameid").GetString(), out _));
        Assert.Contains(jwt.Claims, claim =>
            claim.Type == JwtRegisteredClaimNames.Name &&
            claim.Value == "Santiago Monsalve");
        Assert.Equal(
            HelpDeskApiFactory.AdminEmail,
            payload.GetProperty(JwtRegisteredClaimNames.Email).GetString());
        Assert.Contains(jwt.Claims, claim =>
            claim.Type == "role" &&
            claim.Value == UserRole.SuperAdmin.ToString());
    }

    [Fact]
    public async Task InvalidCredentialsAndInactiveUser_ReturnSameUnauthorizedResponse()
    {
        var adminToken = await Client.LoginAsync();
        Client.UseBearer(adminToken);
        const string inactiveEmail = "inactive@helpdesk.test";
        const string inactivePassword = "Inactive password";
        await Client.CreateUserAsync(
            inactiveEmail,
            inactivePassword,
            UserRole.User);

        var users = await Client.GetFromJsonAsync<JsonElement>(
            $"/api/users?isActive=true&pageSize=100");
        var inactiveId = users.GetProperty("items")
            .EnumerateArray()
            .Single(item => item.GetProperty("email").GetString() == inactiveEmail)
            .GetProperty("id")
            .GetGuid();
        var deactivate = await Client.PatchAsJsonAsync(
            $"/api/users/{inactiveId}/active",
            new { isActive = false });
        Assert.Equal(HttpStatusCode.NoContent, deactivate.StatusCode);
        Client.DefaultRequestHeaders.Authorization = null;

        var missing = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "missing@helpdesk.test", password = "wrong" });
        var wrong = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = HelpDeskApiFactory.AdminEmail, password = "wrong" });
        var inactive = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = inactiveEmail, password = inactivePassword });

        Assert.Equal(HttpStatusCode.Unauthorized, missing.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, inactive.StatusCode);
        var missingBody = await missing.Content.ReadFromJsonAsync<JsonElement>();
        var wrongBody = await wrong.Content.ReadFromJsonAsync<JsonElement>();
        var inactiveBody = await inactive.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(
            missingBody.GetProperty("title").GetString(),
            wrongBody.GetProperty("title").GetString());
        Assert.Equal(
            missingBody.GetProperty("data").GetString(),
            wrongBody.GetProperty("data").GetString());
        Assert.Equal(
            missingBody.GetProperty("data").GetString(),
            inactiveBody.GetProperty("data").GetString());
    }

    [Fact]
    public async Task ProtectedEndpoint_RejectsMissingManipulatedAndExpiredTokens()
    {
        var missing = await Client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, missing.StatusCode);

        Client.UseBearer("header.payload.invalid-signature");
        var manipulated = await Client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, manipulated.StatusCode);

        var expiredToken = CreateExpiredToken();
        Client.UseBearer(expiredToken);
        var expired = await Client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, expired.StatusCode);
    }

    [Fact]
    public async Task UsersEndpoint_EnforcesAuthorizationForAllFourRoles()
    {
        var adminToken = await Client.LoginAsync();
        Client.UseBearer(adminToken);
        var categoryId = await Client.CreateCategoryAsync();
        var credentials = new[]
        {
            (UserRole.User, "user-role@helpdesk.test", (Guid?)null),
            (UserRole.Technician, "technician-role@helpdesk.test", (Guid?)categoryId),
            (UserRole.Supervisor, "supervisor-role@helpdesk.test", (Guid?)categoryId)
        };

        foreach (var credential in credentials)
        {
            await Client.CreateUserAsync(
                credential.Item2,
                "Role password 2026!",
                credential.Item1,
                credential.Item3);
        }

        var adminResponse = await Client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);

        foreach (var credential in credentials)
        {
            Client.DefaultRequestHeaders.Authorization = null;
            var token = await Client.LoginAsync(
                credential.Item2,
                "Role password 2026!");
            Client.UseBearer(token);
            var response = await Client.GetAsync("/api/users");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task Login_AllowsOnlyFiveAttemptsPerMinutePerIp()
    {
        HttpResponseMessage response = null!;
        for (var attempt = 0; attempt < 6; attempt++)
        {
            response = await Client.PostAsJsonAsync(
                "/api/auth/login",
                new { email = "missing@helpdesk.test", password = "wrong" });
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    private static string CreateExpiredToken()
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(HelpDeskApiFactory.SigningKey));
        var token = new JwtSecurityToken(
            issuer: "HelpDesk.Api.Tests",
            audience: "HelpDesk.Api.Tests.Client",
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, UserRole.User.ToString())
            ],
            notBefore: DateTime.UtcNow.AddHours(-2),
            expires: DateTime.UtcNow.AddHours(-1),
            signingCredentials: new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
