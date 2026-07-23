using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Api.Contracts;

public sealed record CreateUserRequest(
    string FullName,
    string Email,
    string Password,
    UserRole Role,
    IReadOnlyCollection<Guid> SupportCategoryIds,
    int? MaxActiveTickets);

public sealed record UpdateUserIdentityRequest(
    string FullName,
    string Email);

public sealed record ResetUserPasswordRequest(string Password);

public sealed record SetActiveRequest(bool IsActive);

public sealed record UpdateTechnicianProfileRequest(
    IReadOnlyCollection<Guid> SupportCategoryIds,
    int MaxActiveTickets);

public sealed record ChangeSupervisorCategoryRequest(Guid SupportCategoryId);
