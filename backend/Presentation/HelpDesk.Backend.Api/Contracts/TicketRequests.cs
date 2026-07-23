using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Api.Contracts;

public sealed record CreateTicketRequest(
    string Subject,
    string Description,
    Guid SupportCategoryId,
    TicketPriority Priority);

public sealed record UpdateTicketRequest(
    string Subject,
    string Description);

public sealed record AddTicketCommentRequest(string Text);

public sealed record AssignTicketRequest(Guid TechnicianUserId);

public sealed record ReassignTicketRequest(
    Guid TechnicianUserId,
    string Reason);

public sealed record ResolveTicketRequest(string ResolutionComment);

public sealed record ForceTicketStatusRequest(
    TicketStatus TargetStatus,
    string Justification);
