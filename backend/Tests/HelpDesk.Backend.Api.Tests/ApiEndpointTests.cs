using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using HelpDesk.Backend.Api.Controllers;
using HelpDesk.Backend.Api.Middleware;
using HelpDesk.Backend.Api.Tests.TestSupport;
using HelpDesk.Backend.Application.Features.Tickets.Models;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace HelpDesk.Backend.Api.Tests;

public sealed class ApiEndpointTests : ApiIntegrationTestBase
{
    [Fact]
    public async Task HealthSwaggerAndCors_AreConfigured()
    {
        var live = await Client.GetAsync("/health/live");
        var ready = await Client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.OK, live.StatusCode);
        Assert.Equal(HttpStatusCode.OK, ready.StatusCode);

        var swagger = await Client.GetFromJsonAsync<JsonElement>(
            "/swagger/v1/swagger.json");
        var schemes = swagger
            .GetProperty("components")
            .GetProperty("securitySchemes");
        Assert.True(schemes.TryGetProperty("Bearer", out var bearer));
        Assert.Equal("http", bearer.GetProperty("type").GetString());
        Assert.Equal("bearer", bearer.GetProperty("scheme").GetString());

        using var preflight = new HttpRequestMessage(HttpMethod.Options, "/api/tickets");
        preflight.Headers.Add("Origin", "http://localhost:5173");
        preflight.Headers.Add("Access-Control-Request-Method", "GET");
        var cors = await Client.SendAsync(preflight);
        Assert.Equal(HttpStatusCode.NoContent, cors.StatusCode);
        Assert.Equal(
            "http://localhost:5173",
            cors.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public async Task ActorIdentifier_ComesFromTokenAndCannotBeForgedByBody()
    {
        var adminToken = await Client.LoginAsync();
        Client.UseBearer(adminToken);
        var categoryId = await Client.CreateCategoryAsync();
        const string email = "ticket-creator@helpdesk.test";
        const string password = "Ticket creator password";
        await Client.CreateUserAsync(email, password, UserRole.User);

        var users = await Client.GetFromJsonAsync<JsonElement>(
            "/api/users?pageSize=100");
        var creatorId = users.GetProperty("items")
            .EnumerateArray()
            .Single(item => item.GetProperty("email").GetString() == email)
            .GetProperty("id")
            .GetGuid();

        Client.DefaultRequestHeaders.Authorization = null;
        Client.UseBearer(await Client.LoginAsync(email, password));
        var create = await Client.PostAsJsonAsync(
            "/api/tickets",
            new
            {
                actorUserId = Guid.NewGuid(),
                subject = "Solicitud creada desde la API",
                description = "El actor debe provenir exclusivamente del token.",
                supportCategoryId = categoryId,
                priority = "high"
            });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<CreatedTicketResponse>();

        var detail = await Client.GetFromJsonAsync<JsonElement>(
            $"/api/tickets/{created!.TicketId}");
        Assert.Equal(creatorId, detail.GetProperty("creatorUserId").GetGuid());
    }

    [Fact]
    public async Task CategorySlaMinutes_AcceptsDurationsGreaterThanTwentyFourHours()
    {
        Client.UseBearer(await Client.LoginAsync());
        var categoryId = await Client.CreateCategoryAsync();

        var detail = await Client.GetFromJsonAsync<JsonElement>(
            $"/api/support-categories/{categoryId}");
        var low = detail.GetProperty("slaPolicies")
            .EnumerateArray()
            .Single(policy => policy.GetProperty("priority").GetString() == "low");

        Assert.Equal("1.00:00:00", low.GetProperty("responseTime").GetString());
    }

    [Fact]
    public async Task ApiMapsBadRequestValidationNotFoundAndConflict()
    {
        Client.UseBearer(await Client.LoginAsync());
        var categoryId = await Client.CreateCategoryAsync();

        var numericEnum = await Client.PostAsJsonAsync(
            "/api/tickets",
            new
            {
                subject = "Asunto",
                description = "Descripción",
                supportCategoryId = categoryId,
                priority = 2
            });
        Assert.Equal(HttpStatusCode.BadRequest, numericEnum.StatusCode);

        var validation = await Client.PostAsJsonAsync(
            "/api/tickets",
            new
            {
                subject = "",
                description = "",
                supportCategoryId = categoryId,
                priority = "high"
            });
        Assert.Equal(
            HttpStatusCode.UnprocessableEntity,
            validation.StatusCode);

        var missing = await Client.GetAsync($"/api/tickets/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

        var categoryRequest = new
        {
            name = "Categoría única",
            description = "Descripción",
            lowSlaMinutes = 1440,
            mediumSlaMinutes = 480,
            highSlaMinutes = 240,
            criticalSlaMinutes = 120
        };
        var first = await Client.PostAsJsonAsync(
            "/api/support-categories",
            categoryRequest);
        var duplicate = await Client.PostAsJsonAsync(
            "/api/support-categories",
            categoryRequest);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public void Controllers_AreThinAndDoNotDependOnDbContext()
    {
        var controllers = new[]
        {
            typeof(AuthController),
            typeof(UsersController),
            typeof(SupportCategoriesController),
            typeof(TicketsController),
            typeof(SlaController)
        };

        foreach (var controller in controllers)
        {
            var parameters = controller
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Single()
                .GetParameters();
            Assert.Single(parameters);
            Assert.Equal(typeof(ISender), parameters[0].ParameterType);
            Assert.DoesNotContain(
                parameters,
                parameter => parameter.ParameterType == typeof(HelpDeskDbContext));
        }
    }

    [Fact]
    public async Task UnexpectedException_UsesOpiErrorContractWithoutDetails()
    {
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "trace-api-test";
        context.Response.Body = new MemoryStream();
        var middleware = new GlobalExceptionMiddleware(
            _ => throw new Exception("sensitive detail"),
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(
            context.Response.Body,
            Encoding.UTF8);
        var body = await reader.ReadToEndAsync();
        Assert.Contains("\"traceId\":\"trace-api-test\"", body);
        Assert.DoesNotContain("sensitive detail", body);
    }

    [Fact]
    public async Task ClientCancellation_DoesNotBecomeAnInternalServerError()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var context = new DefaultHttpContext();
        context.RequestAborted = cancellation.Token;
        context.Response.Body = new MemoryStream();
        var middleware = new GlobalExceptionMiddleware(
            _ => throw new OperationCanceledException(cancellation.Token),
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.NotEqual(
            StatusCodes.Status500InternalServerError,
            context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
    }

    [Fact]
    public async Task OpenApiContainsAllPublicRoutesAndExcludesInternalJobs()
    {
        var swagger = await Client.GetFromJsonAsync<JsonElement>(
            "/swagger/v1/swagger.json");
        var paths = swagger.GetProperty("paths");
        var expected = new[]
        {
            "/api/auth/login",
            "/api/auth/me",
            "/api/users",
            "/api/support-categories",
            "/api/tickets",
            "/api/tickets/{id}/assign",
            "/api/tickets/{id}/reassign",
            "/api/tickets/{id}/force-status",
            "/api/sla/alerts",
            "/api/sla/report"
        };

        foreach (var path in expected)
        {
            Assert.True(paths.TryGetProperty(path, out _), path);
        }

        Assert.DoesNotContain(
            paths.EnumerateObject(),
            path => path.Name.Contains("evaluate", StringComparison.OrdinalIgnoreCase) ||
                path.Name.Contains("automatic", StringComparison.OrdinalIgnoreCase));
    }
}
