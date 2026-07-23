using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Domain.Categories;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Tickets;
using HelpDesk.Backend.Domain.Users;
using HelpDesk.Backend.Application.Abstractions.Queries;

namespace HelpDesk.Backend.Application.Common;

internal static class ApplicationAccess
{
    internal static async Task<User> GetUserAsync(
        IUnitOfWork unitOfWork,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("No se encontró el usuario solicitado.");
    }

    internal static async Task<Ticket> GetTicketAsync(
        IUnitOfWork unitOfWork,
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        return await unitOfWork.Tickets.GetByIdAsync(ticketId, cancellationToken)
            ?? throw new KeyNotFoundException("No se encontró el ticket solicitado.");
    }

    internal static async Task<SupportCategory> GetSupportCategoryAsync(
        IUnitOfWork unitOfWork,
        Guid supportCategoryId,
        CancellationToken cancellationToken)
    {
        return await unitOfWork.SupportCategories.GetByIdAsync(supportCategoryId, cancellationToken)
            ?? throw new KeyNotFoundException("No se encontró la categoría solicitada.");
    }

    internal static void EnsureSuperAdmin(User actor)
    {
        if (!actor.IsActive || actor.Role != UserRole.SuperAdmin)
        {
            throw new UnauthorizedAccessException("La operación requiere un SuperAdmin activo.");
        }
    }

    internal static TicketVisibilityScope CreateTicketVisibilityScope(User actor)
    {
        if (!actor.IsActive)
        {
            throw new UnauthorizedAccessException("La operación requiere un usuario activo.");
        }

        return new TicketVisibilityScope(
            actor.Id,
            actor.Role,
            actor.SupervisorProfile?.SupportCategoryId);
    }
}
