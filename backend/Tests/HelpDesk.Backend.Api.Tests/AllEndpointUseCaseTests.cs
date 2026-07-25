using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HelpDesk.Backend.Api.Tests.TestSupport;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Api.Tests;

public sealed class AllEndpointUseCaseTests : ApiIntegrationTestBase
{
    private const string UserPassword = "Santiago password 2026!";
    private const string TechnicianPassword = "Juan password 2026!";

    [Fact]
    public async Task UserAndCategoryEndpoints_ExecuteEverySupportedUseCase()
    {
        Client.UseBearer(await Client.LoginAsync());

        var me = await Client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);

        var hardwareId = await Client.CreateCategoryAsync();
        var softwareId = await Client.CreateCategoryAsync(
            "Soporte de software",
            "Atención de aplicaciones, correo y accesos.");

        Assert.Equal(
            HttpStatusCode.OK,
            (await Client.GetAsync("/api/support-categories")).StatusCode);
        Assert.Equal(
            HttpStatusCode.OK,
            (await Client.GetAsync($"/api/support-categories/{hardwareId}")).StatusCode);

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PutAsJsonAsync(
                $"/api/support-categories/{hardwareId}",
                new
                {
                    name = "Mesa de ayuda de hardware",
                    description = "Atención de equipos, impresoras y periféricos."
                })).StatusCode);
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PutAsJsonAsync(
                $"/api/support-categories/{hardwareId}/sla/low",
                new { responseTimeMinutes = 2880 })).StatusCode);
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PatchAsJsonAsync(
                $"/api/support-categories/{hardwareId}/active",
                new { isActive = false })).StatusCode);
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PatchAsJsonAsync(
                $"/api/support-categories/{hardwareId}/active",
                new { isActive = true })).StatusCode);

        var userId = await Client.CreateUserAsync(
            "santiago.usuario@helpdesk.test",
            UserPassword,
            UserRole.User,
            fullName: "Santiago Monsalve");
        var technicianId = await Client.CreateUserAsync(
            "juan.reyes@helpdesk.test",
            TechnicianPassword,
            UserRole.Technician,
            hardwareId,
            "Juan Reyes",
            technicianCapacity: 1);
        var supervisorId = await Client.CreateUserAsync(
            "sergio.otalvaro@helpdesk.test",
            "Sergio password 2026!",
            UserRole.Supervisor,
            hardwareId,
            "Sergio Otalvaro");

        Assert.Equal(
            HttpStatusCode.OK,
            (await Client.GetAsync("/api/users?pageSize=100")).StatusCode);
        Assert.Equal(
            HttpStatusCode.OK,
            (await Client.GetAsync($"/api/users/{technicianId}")).StatusCode);

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PutAsJsonAsync(
                $"/api/users/{userId}/identity",
                new
                {
                    fullName = "Santiago Monsalve Restrepo",
                    email = "santiago.monsalve.rest@helpdesk.test"
                })).StatusCode);
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PutAsJsonAsync(
                $"/api/users/{userId}/password",
                new { password = "Nueva clave Santiago 2026!" })).StatusCode);
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PatchAsJsonAsync(
                $"/api/users/{userId}/active",
                new { isActive = false })).StatusCode);
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PatchAsJsonAsync(
                $"/api/users/{userId}/active",
                new { isActive = true })).StatusCode);
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PutAsJsonAsync(
                $"/api/users/{technicianId}/technician-profile",
                new
                {
                    supportCategoryIds = new[] { hardwareId, softwareId },
                    maxActiveTickets = 2
                })).StatusCode);
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PutAsJsonAsync(
                $"/api/users/{supervisorId}/supervisor-category",
                new { supportCategoryId = softwareId })).StatusCode);
    }

    [Fact]
    public async Task TicketEndpoints_ExecuteCompleteAssignmentResolutionAndAdministrativeFlows()
    {
        var adminToken = await Client.LoginAsync();
        Client.UseBearer(adminToken);
        var categoryId = await Client.CreateCategoryAsync();
        await Client.CreateUserAsync(
            "santiago.usuario@helpdesk.test",
            UserPassword,
            UserRole.User,
            fullName: "Santiago Monsalve");
        var juanId = await Client.CreateUserAsync(
            "juan.reyes@helpdesk.test",
            TechnicianPassword,
            UserRole.Technician,
            categoryId,
            "Juan Reyes",
            technicianCapacity: 3);
        var lauraId = await Client.CreateUserAsync(
            "laura.gomez@helpdesk.test",
            "Laura password 2026!",
            UserRole.Technician,
            categoryId,
            "Laura Gómez",
            technicianCapacity: 3);

        var creatorToken = await Client.LoginAsync(
            "santiago.usuario@helpdesk.test",
            UserPassword);
        Client.UseBearer(creatorToken);
        var ticket = await Client.CreateTicketAsync(categoryId);

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PutAsJsonAsync(
                $"/api/tickets/{ticket.TicketId}",
                new
                {
                    subject = "Falla en computador de contabilidad",
                    description = "Este es un ticket de hardware para revisar el disco del equipo."
                })).StatusCode);
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PostAsJsonAsync(
                $"/api/tickets/{ticket.TicketId}/comments",
                new { text = "El equipo presenta la falla desde esta mañana." })).StatusCode);

        Client.UseBearer(adminToken);
        Assert.Equal(
            HttpStatusCode.OK,
            (await Client.GetAsync(
                $"/api/tickets/{ticket.TicketId}/assignable-technicians")).StatusCode);
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PostAsJsonAsync(
                $"/api/tickets/{ticket.TicketId}/assign",
                new { technicianUserId = juanId })).StatusCode);

        var beforeReassignment = await Client.GetFromJsonAsync<JsonElement>(
            $"/api/tickets/{ticket.TicketId}");
        var deadlineBefore = CurrentSlaDeadline(beforeReassignment);

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PostAsJsonAsync(
                $"/api/tickets/{ticket.TicketId}/reassign",
                new
                {
                    technicianUserId = lauraId,
                    reason = "Laura Gómez continuará la revisión del equipo."
                })).StatusCode);

        var afterReassignment = await Client.GetFromJsonAsync<JsonElement>(
            $"/api/tickets/{ticket.TicketId}");
        Assert.Equal(deadlineBefore, CurrentSlaDeadline(afterReassignment));
        Assert.Equal(2, afterReassignment.GetProperty("assignments").GetArrayLength());

        var lauraToken = await Client.LoginAsync(
            "laura.gomez@helpdesk.test",
            "Laura password 2026!");
        Client.UseBearer(lauraToken);
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PostAsync(
                $"/api/tickets/{ticket.TicketId}/start",
                null)).StatusCode);
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PostAsJsonAsync(
                $"/api/tickets/{ticket.TicketId}/resolve",
                new
                {
                    resolutionComment =
                        "Se reemplazó el disco y el computador quedó funcionando correctamente."
                })).StatusCode);

        Client.UseBearer(creatorToken);
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PostAsync(
                $"/api/tickets/{ticket.TicketId}/close",
                null)).StatusCode);

        var removable = await Client.CreateTicketAsync(
            categoryId,
            "Consulta sobre teclado",
            "Este es un ticket de hardware que todavía no ha sido asignado.",
            "low");
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.DeleteAsync($"/api/tickets/{removable.TicketId}")).StatusCode);

        var administrative = await Client.CreateTicketAsync(
            categoryId,
            "Falla intermitente de impresora",
            "La impresora de recepción deja de responder.",
            "medium");
        Client.UseBearer(adminToken);
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PostAsJsonAsync(
                $"/api/tickets/{administrative.TicketId}/assign",
                new { technicianUserId = juanId })).StatusCode);
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await Client.PostAsJsonAsync(
                $"/api/tickets/{administrative.TicketId}/force-status",
                new
                {
                    targetStatus = "resolved",
                    justification =
                        "El supervisor verificó la impresora y confirmó que el servicio quedó estable."
                })).StatusCode);

        Assert.Equal(
            HttpStatusCode.OK,
            (await Client.GetAsync("/api/tickets?pageSize=100")).StatusCode);
        Assert.Equal(
            HttpStatusCode.OK,
            (await Client.GetAsync($"/api/tickets/{administrative.TicketId}")).StatusCode);
    }

    [Fact]
    public async Task ReopenEndpoint_WhenPreviousTechnicianIsFull_LeavesTicketUnassigned()
    {
        var adminToken = await Client.LoginAsync();
        Client.UseBearer(adminToken);
        var categoryId = await Client.CreateCategoryAsync(
            criticalSlaMinutes: 60);
        await Client.CreateUserAsync(
            "santiago.usuario@helpdesk.test",
            UserPassword,
            UserRole.User,
            fullName: "Santiago Monsalve");
        var technicianId = await Client.CreateUserAsync(
            "juan.reyes@helpdesk.test",
            TechnicianPassword,
            UserRole.Technician,
            categoryId,
            "Juan Reyes",
            technicianCapacity: 1);

        var creatorToken = await Client.LoginAsync(
            "santiago.usuario@helpdesk.test",
            UserPassword);
        var technicianToken = await Client.LoginAsync(
            "juan.reyes@helpdesk.test",
            TechnicianPassword);

        Client.UseBearer(creatorToken);
        var resolvedTicket = await Client.CreateTicketAsync(
            categoryId,
            "Equipo no enciende",
            "Este es un ticket de hardware para revisar la fuente de poder.",
            "critical");

        Client.UseBearer(adminToken);
        await AssertNoContentAsync(
            Client.PostAsJsonAsync(
                $"/api/tickets/{resolvedTicket.TicketId}/assign",
                new { technicianUserId = technicianId }));

        Client.UseBearer(technicianToken);
        await AssertNoContentAsync(
            Client.PostAsync($"/api/tickets/{resolvedTicket.TicketId}/start", null));
        await AssertNoContentAsync(
            Client.PostAsJsonAsync(
                $"/api/tickets/{resolvedTicket.TicketId}/resolve",
                new { resolutionComment = "Se cambió la fuente de poder del equipo." }));

        Client.UseBearer(creatorToken);
        var capacityTicket = await Client.CreateTicketAsync(
            categoryId,
            "Monitor sin imagen",
            "Este es un ticket de hardware para revisar el cable de video.",
            "high");

        Client.UseBearer(adminToken);
        await AssertNoContentAsync(
            Client.PostAsJsonAsync(
                $"/api/tickets/{capacityTicket.TicketId}/assign",
                new { technicianUserId = technicianId }));

        Client.UseBearer(creatorToken);
        await AssertNoContentAsync(
            Client.PostAsync($"/api/tickets/{resolvedTicket.TicketId}/reopen", null));

        var reopened = await Client.GetFromJsonAsync<JsonElement>(
            $"/api/tickets/{resolvedTicket.TicketId}");
        Assert.Equal("reopened", reopened.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, reopened.GetProperty("currentTechnicianUserId").ValueKind);
        Assert.Equal(2, reopened.GetProperty("slaCycles").GetArrayLength());
        Assert.DoesNotContain(
            reopened.GetProperty("assignments").EnumerateArray(),
            assignment => assignment.GetProperty("isCurrent").GetBoolean());

        var rejectedTicket = await Client.CreateTicketAsync(
            categoryId,
            "Mouse sin respuesta",
            "Este es un ticket de hardware para revisar el puerto USB.",
            "low");
        Client.UseBearer(adminToken);
        var capacityRejected = await Client.PostAsJsonAsync(
            $"/api/tickets/{rejectedTicket.TicketId}/assign",
            new { technicianUserId = technicianId });
        Assert.Equal(HttpStatusCode.Conflict, capacityRejected.StatusCode);
        var error = await capacityRejected.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(
            "TECHNICIAN_AT_CAPACITY",
            error.GetProperty("errors")[0].GetProperty("code").GetString());
    }

    [Fact]
    public async Task SlaEndpoints_ReportBreachAndResponsibleTechnician()
    {
        var adminToken = await Client.LoginAsync();
        Client.UseBearer(adminToken);
        var categoryId = await Client.CreateCategoryAsync(
            criticalSlaMinutes: 1);
        await Client.CreateUserAsync(
            "santiago.usuario@helpdesk.test",
            UserPassword,
            UserRole.User,
            fullName: "Santiago Monsalve");
        var technicianId = await Client.CreateUserAsync(
            "juan.reyes@helpdesk.test",
            TechnicianPassword,
            UserRole.Technician,
            categoryId,
            "Juan Reyes",
            technicianCapacity: 2);

        var creatorToken = await Client.LoginAsync(
            "santiago.usuario@helpdesk.test",
            UserPassword);
        Client.UseBearer(creatorToken);
        var ticket = await Client.CreateTicketAsync(
            categoryId,
            "Servidor sin acceso a red",
            "Este es un ticket crítico de hardware para revisar la tarjeta de red.",
            "critical");

        Client.UseBearer(adminToken);
        await AssertNoContentAsync(
            Client.PostAsJsonAsync(
                $"/api/tickets/{ticket.TicketId}/assign",
                new { technicianUserId = technicianId }));

        Factory.Clock.Advance(TimeSpan.FromMinutes(2));
        var technicianToken = await Client.LoginAsync(
            "juan.reyes@helpdesk.test",
            TechnicianPassword);
        Client.UseBearer(technicianToken);
        await AssertNoContentAsync(
            Client.PostAsync($"/api/tickets/{ticket.TicketId}/start", null));

        adminToken = await Client.LoginAsync();
        Client.UseBearer(adminToken);
        var detail = await Client.GetFromJsonAsync<JsonElement>(
            $"/api/tickets/{ticket.TicketId}");
        var cycle = detail.GetProperty("slaCycles")[0];
        Assert.Equal("breached", cycle.GetProperty("outcome").GetString());
        Assert.Equal(
            technicianId,
            cycle.GetProperty("responsibleTechnicianUserId").GetGuid());

        var alerts = await Client.GetAsync(
            $"/api/sla/alerts?supportCategoryId={categoryId}");
        var report = await Client.GetFromJsonAsync<JsonElement>(
            $"/api/sla/report?supportCategoryId={categoryId}&technicianUserId={technicianId}");
        Assert.Equal(HttpStatusCode.OK, alerts.StatusCode);
        Assert.Equal(1, report.GetProperty("totalBreachedCycles").GetInt32());
        Assert.Equal(0m, report.GetProperty("compliancePercentage").GetDecimal());
    }

    private static DateTimeOffset CurrentSlaDeadline(JsonElement ticket)
    {
        var cycles = ticket.GetProperty("slaCycles").EnumerateArray().ToArray();
        return cycles[^1].GetProperty("deadlineAtUtc").GetDateTimeOffset();
    }

    private static async Task AssertNoContentAsync(Task<HttpResponseMessage> request)
    {
        using var response = await request;
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
