using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Resources;
using HelpDesk.Backend.Domain.Aggregates.SupportCategories;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;
using HelpDesk.Backend.Domain.Aggregates.Users;
using HelpDesk.Backend.Application.Interfaces.Queries;

namespace HelpDesk.Backend.Application.Common.Authorization;

internal static class ApplicationAccess
{
    internal static async Task<User> GetUserAsync(
        IUnitOfWork unitOfWork,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException(ApplicationMessages.UserNotFound);
    }

    internal static async Task<Ticket> GetTicketAsync(
        IUnitOfWork unitOfWork,
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        return await unitOfWork.Tickets.GetByIdAsync(ticketId, cancellationToken)
            ?? throw new KeyNotFoundException(ApplicationMessages.TicketNotFound);
    }

    internal static async Task<SupportCategory> GetSupportCategoryAsync(
        IUnitOfWork unitOfWork,
        Guid supportCategoryId,
        CancellationToken cancellationToken)
    {
        return await unitOfWork.SupportCategories.GetByIdAsync(supportCategoryId, cancellationToken)
            ?? throw new KeyNotFoundException(ApplicationMessages.SupportCategoryNotFound);
    }

    internal static void EnsureSuperAdmin(User actor)
    {
        if (!actor.IsActive || actor.Role != UserRole.SuperAdmin)
        {
            throw new UnauthorizedAccessException(ApplicationMessages.ActiveSuperAdminRequired);
        }
    }

    internal static TicketVisibilityScope CreateTicketVisibilityScope(User actor)
    {
        if (!actor.IsActive)
        {
            throw new UnauthorizedAccessException(ApplicationMessages.ActiveUserRequired);
        }

        return new TicketVisibilityScope(
            actor.Id,
            actor.Role,
            actor.SupervisorProfile?.SupportCategoryId);
    }
}
