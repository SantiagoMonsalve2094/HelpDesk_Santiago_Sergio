using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Api.DTOs.Tickets;

public sealed record ForceTicketStatusApiRequest(
    TicketStatus TargetStatus,
    string Justification);
