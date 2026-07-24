using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Application.DTOs.Tickets;

public sealed record TicketVisibilityScope(
    Guid ActorUserId,
    UserRole ActorRole,
    Guid? SupervisorSupportCategoryId);
