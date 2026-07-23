namespace HelpDesk.Backend.Api.Authorization;

internal static class RoleNames
{
    internal const string SuperAdmin = "SuperAdmin";
    internal const string SupervisorOrSuperAdmin = "Supervisor,SuperAdmin";
    internal const string TechnicianSupervisorOrSuperAdmin =
        "Technician,Supervisor,SuperAdmin";
}
