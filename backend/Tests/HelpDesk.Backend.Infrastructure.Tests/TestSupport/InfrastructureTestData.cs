using HelpDesk.Backend.Domain.Aggregates.SupportCategories;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;
using HelpDesk.Backend.Domain.Aggregates.Users;
using HelpDesk.Backend.Domain.ValueObjects;

namespace HelpDesk.Backend.Infrastructure.Tests.TestSupport;

internal static class InfrastructureTestData
{
    internal static readonly DateTimeOffset Now =
        new(2026, 7, 23, 12, 0, 0, TimeSpan.Zero);

    internal static SupportCategory CreateCategory(string name) =>
        SupportCategory.Create(
            name,
            $"Descripción de {name}",
            new Dictionary<TicketPriority, TimeSpan>
            {
                [TicketPriority.Low] = TimeSpan.FromHours(36),
                [TicketPriority.Medium] = TimeSpan.FromHours(12),
                [TicketPriority.High] = TimeSpan.FromHours(4),
                [TicketPriority.Critical] = TimeSpan.FromHours(1)
            },
            Now);

    internal static User CreateUser(string name, string email) =>
        User.CreateUser(name, email, $"hash-{Guid.NewGuid():N}", Now);

    internal static User CreateTechnician(
        string name,
        string email,
        IEnumerable<Guid> categoryIds,
        int capacity = 5) =>
        User.CreateTechnician(
            name,
            email,
            $"hash-{Guid.NewGuid():N}",
            categoryIds,
            capacity,
            Now);

    internal static Ticket CreateTicket(
        int sequence,
        User creator,
        SupportCategory category,
        TicketPriority priority = TicketPriority.Critical,
        DateTimeOffset? now = null) =>
        Ticket.Create(
            TicketNumber.Create(Now.Year, sequence),
            $"Ticket {sequence}",
            $"Descripción del ticket {sequence}",
            creator.Id,
            category.Id,
            priority,
            category.GetSlaDuration(priority),
            now ?? Now);
}
