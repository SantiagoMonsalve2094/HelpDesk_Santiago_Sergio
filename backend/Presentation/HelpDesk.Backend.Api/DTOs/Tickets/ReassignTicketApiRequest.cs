using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Api.DTOs.Tickets;

public sealed record ReassignTicketApiRequest(
    Guid TechnicianUserId,
    string Reason);
