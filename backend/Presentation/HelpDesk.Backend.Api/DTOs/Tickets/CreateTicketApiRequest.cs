using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Api.DTOs.Tickets;

public sealed record CreateTicketApiRequest(
    string Subject,
    string Description,
    Guid SupportCategoryId,
    TicketPriority Priority);
