using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Users;

namespace HelpDesk.Backend.Application.DTOs.Users;

public sealed record UserSummaryResponse(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive);
