using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;

namespace HelpDesk.Backend.Application.DTOs.Tickets;

public sealed record AssignableTechnicianResponse(
    Guid TechnicianUserId,
    string FullName,
    int MaxActiveTickets,
    int ActiveTicketCount,
    int AvailableCapacity);
