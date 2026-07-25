using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Users;

namespace HelpDesk.Backend.Application.DTOs.Users;

public sealed record UserDetailsResponse(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive,
    TechnicianProfileResponse? TechnicianProfile,
    SupervisorProfileResponse? SupervisorProfile,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
