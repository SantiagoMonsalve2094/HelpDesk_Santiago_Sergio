using HelpDesk.Backend.Domain.Aggregates.SupportCategories;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;
using HelpDesk.Backend.Domain.Aggregates.Users;
using HelpDesk.Backend.Domain.ValueObjects;

namespace HelpDesk.Backend.Application.Tests.Common.TestDoubles;

internal static class ApplicationTestData
{
    internal static readonly DateTimeOffset Now =
        new(2026, 7, 23, 15, 0, 0, TimeSpan.Zero);

    internal static IReadOnlyDictionary<TicketPriority, TimeSpan> CompleteSla() =>
        new Dictionary<TicketPriority, TimeSpan>
        {
            [TicketPriority.Low] = TimeSpan.FromHours(24),
            [TicketPriority.Medium] = TimeSpan.FromHours(12),
            [TicketPriority.High] = TimeSpan.FromHours(6),
            [TicketPriority.Critical] = TimeSpan.FromHours(2)
        };

    internal static SupportCategory Category(string? name = null) =>
        SupportCategory.Create(
            name ?? $"Categoría {Guid.NewGuid():N}",
            "Descripción de la categoría",
            CompleteSla(),
            Now);

    internal static User User(string? email = null) =>
        HelpDesk.Backend.Domain.Aggregates.Users.User.CreateUser(
            "Santiago Monsalve",
            email ?? $"santiago-{Guid.NewGuid():N}@helpdesk.test",
            "hash-user",
            Now);

    internal static User Technician(
        IEnumerable<Guid> categoryIds,
        int capacity = 3,
        string? email = null) =>
        HelpDesk.Backend.Domain.Aggregates.Users.User.CreateTechnician(
            "Juan Reyes",
            email ?? $"juan-{Guid.NewGuid():N}@helpdesk.test",
            "hash-tech",
            categoryIds,
            capacity,
            Now);

    internal static User Supervisor(Guid categoryId) =>
        HelpDesk.Backend.Domain.Aggregates.Users.User.CreateSupervisor(
            "Sergio Otalvaro",
            $"sergio-{Guid.NewGuid():N}@helpdesk.test",
            "hash-supervisor",
            categoryId,
            Now);

    internal static User SuperAdmin() =>
        HelpDesk.Backend.Domain.Aggregates.Users.User.CreateSuperAdmin(
            "Santiago Monsalve",
            $"santiago.admin-{Guid.NewGuid():N}@helpdesk.test",
            "hash-admin",
            Now);

    internal static Ticket Ticket(
        Guid creatorUserId,
        Guid categoryId,
        TicketPriority priority = TicketPriority.Critical,
        int sequence = 1) =>
        HelpDesk.Backend.Domain.Aggregates.Tickets.Ticket.Create(
            TicketNumber.Create(2026, sequence),
            "Equipo sin acceso a red",
            "El equipo no logra conectarse a la red corporativa.",
            creatorUserId,
            categoryId,
            priority,
            CompleteSla()[priority],
            Now);
}
