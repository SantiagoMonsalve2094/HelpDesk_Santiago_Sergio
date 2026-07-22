using HelpDesk.Backend.Domain.Categories;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Tickets;
using HelpDesk.Backend.Domain.Users;
using HelpDesk.Backend.Domain.ValueObjects;

namespace HelpDesk.Backend.Domain.Tests;

internal static class TestData
{
    public static readonly DateTimeOffset Now = new(2026, 7, 22, 15, 0, 0, TimeSpan.Zero);

    public static IReadOnlyDictionary<TicketPriority, TimeSpan> CompleteSla() =>
        new Dictionary<TicketPriority, TimeSpan>
        {
            [TicketPriority.Low] = TimeSpan.FromHours(24),
            [TicketPriority.Medium] = TimeSpan.FromHours(12),
            [TicketPriority.High] = TimeSpan.FromHours(6),
            [TicketPriority.Critical] = TimeSpan.FromHours(2)
        };

    public static SupportCategory Category(Guid? id = null)
    {
        var category = SupportCategory.Create("Hardware", "Soporte de equipos", CompleteSla(), Now);
        return category;
    }

    public static User NormalUser(Guid? ignored = null) =>
        User.CreateUser("Usuario Normal", "user@example.com", "hash-user", Now);

    public static User Technician(IEnumerable<Guid> categoryIds, int capacity = 3) =>
        User.CreateTechnician(
            "Técnico Uno",
            $"tech-{Guid.NewGuid():N}@example.com",
            "hash-tech",
            categoryIds,
            capacity,
            Now);

    public static User Supervisor(Guid categoryId) =>
        User.CreateSupervisor(
            "Supervisor Uno",
            $"supervisor-{Guid.NewGuid():N}@example.com",
            "hash-supervisor",
            categoryId,
            Now);

    public static User SuperAdmin() =>
        User.CreateSuperAdmin(
            "Administrador",
            $"admin-{Guid.NewGuid():N}@example.com",
            "hash-admin",
            Now);

    public static Ticket Ticket(
        Guid creatorUserId,
        Guid categoryId,
        TimeSpan? slaDuration = null) =>
        HelpDesk.Backend.Domain.Tickets.Ticket.Create(
            TicketNumber.Create(2026, 1),
            "Equipo sin acceso a red",
            "El equipo no logra conectarse a la red corporativa.",
            creatorUserId,
            categoryId,
            TicketPriority.Critical,
            slaDuration ?? TimeSpan.FromHours(2),
            Now);
}
