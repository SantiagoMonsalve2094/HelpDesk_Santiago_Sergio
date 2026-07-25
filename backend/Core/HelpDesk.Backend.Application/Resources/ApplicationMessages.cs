namespace HelpDesk.Backend.Application.Resources;

public static class ApplicationMessages
{
    public const string InvalidCredentials =
        "El email o la contraseña son incorrectos, o el usuario está inactivo.";
    public const string UserNotFound = "No se encontró el usuario solicitado.";
    public const string TicketNotFound = "No se encontró el ticket solicitado.";
    public const string SupportCategoryNotFound = "No se encontró la categoría solicitada.";
    public const string AssignedSupportCategoryNotFound =
        "No se encontró una categoría asignada al usuario.";
    public const string ActiveSuperAdminRequired =
        "La operación requiere un SuperAdmin activo.";
    public const string ActiveUserRequired = "La operación requiere un usuario activo.";
    public const string OnlySuperAdminCanViewInactiveCategories =
        "Solo un SuperAdmin puede consultar categorías inactivas.";
    public const string UserInactive = "El usuario está inactivo.";
    public const string UserEmailAlreadyExists =
        "Ya existe un usuario con el email indicado.";
    public const string SupportCategoryNameAlreadyExists =
        "Ya existe una categoría con el nombre indicado.";
    public const string InactiveSupportCategoryCannotBeAssigned =
        "No se puede asignar una categoría inactiva.";
    public const string UserIsNotTechnician = "El usuario no tiene perfil de técnico.";
    public const string UserIsNotSupervisor = "El usuario no tiene perfil de supervisor.";
    public const string InvalidUserRole = "El rol indicado no es válido.";
    public const string InvalidUserProfile =
        "Las categorías y la capacidad no corresponden al rol solicitado.";
    public const string OnlyActiveUserCanCreateTickets =
        "Solo un usuario activo puede crear tickets.";
    public const string OnlyActiveSuperAdminCanCreateCategories =
        "Solo un SuperAdmin activo puede crear categorías.";
    public const string CannotViewTicket = "El usuario no puede consultar este ticket.";
    public const string CannotManageTicketAssignment =
        "El usuario no puede administrar asignaciones de este ticket.";
    public const string CannotStartOrResolveTicket =
        "El usuario no puede cambiar la atención de este ticket.";
    public const string CannotCommentTicket = "El usuario no puede comentar este ticket.";
    public const string CannotForceTicketStatus =
        "El usuario no puede forzar el estado de este ticket.";
    public const string CannotModifyCategorySla =
        "El usuario no puede modificar el SLA de esta categoría.";
    public const string SlaReportRequiresSupervisorOrSuperAdmin =
        "El reporte SLA requiere un Supervisor o SuperAdmin activo.";
    public const string SupervisorCanOnlyViewOwnCategoryReport =
        "El supervisor solo puede consultar el reporte de su categoría.";
    public const string DateRangeInvalid =
        "La fecha inicial no puede ser posterior a la fecha final.";
}
