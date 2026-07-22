using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Tickets;
using HelpDesk.Backend.Domain.Users;

namespace HelpDesk.Backend.Domain.Policies;

public static class TicketAssignmentPolicy
{
    public static void EnsureCanAssign(
        User actor,
        Ticket ticket,
        User technician,
        int technicianActiveTicketCount)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(ticket);
        ArgumentNullException.ThrowIfNull(technician);

        if (!TicketAccessPolicy.CanManageAssignment(actor, ticket))
        {
            throw new DomainException("TICKET_ASSIGNMENT_FORBIDDEN", "El usuario no puede asignar tickets de esta categoría.");
        }

        if (!technician.IsActive || technician.Role != UserRole.Technician || technician.TechnicianProfile is null)
        {
            throw new DomainException("INVALID_TECHNICIAN", "El usuario seleccionado no es un técnico activo.");
        }

        if (!technician.SupportsCategory(ticket.SupportCategoryId))
        {
            throw new DomainException("TECHNICIAN_NOT_QUALIFIED", "El técnico no está habilitado para la categoría del ticket.");
        }

        if (technicianActiveTicketCount < 0)
        {
            throw new DomainException("INVALID_ACTIVE_TICKET_COUNT", "La cantidad de tickets activos no puede ser negativa.");
        }

        if (technicianActiveTicketCount >= technician.TechnicianProfile.MaxActiveTickets)
        {
            throw new DomainException("TECHNICIAN_AT_CAPACITY", "El técnico alcanzó su capacidad máxima de tickets activos.");
        }
    }
}
