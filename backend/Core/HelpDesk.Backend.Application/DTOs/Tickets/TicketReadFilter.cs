using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Application.DTOs.Tickets;

public sealed record TicketReadFilter(
    TicketVisibilityScope Visibility,
    TicketStatus? Status,
    TicketPriority? Priority,
    Guid? SupportCategoryId,
    Guid? TechnicianUserId,
    bool? IsOverdue,
    DateTimeOffset? CreatedFromUtc,
    DateTimeOffset? CreatedToUtc);
