using HelpDesk.Backend.Application.DTOs.Users;
using HelpDesk.Backend.Application.Features.Users;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Users;

namespace HelpDesk.Backend.Application.DTOs.Auth;

public sealed record AuthenticatedUserResponse(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    TechnicianProfileResponse? TechnicianProfile,
    SupervisorProfileResponse? SupervisorProfile);
