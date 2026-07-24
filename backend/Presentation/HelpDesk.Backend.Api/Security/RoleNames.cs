namespace HelpDesk.Backend.Api.Security;

internal static class RoleNames
{
    internal const string SuperAdmin = "SuperAdmin";
    internal const string SupervisorOrSuperAdmin = "Supervisor,SuperAdmin";
    internal const string TechnicianSupervisorOrSuperAdmin =
        "Technician,Supervisor,SuperAdmin";
}
