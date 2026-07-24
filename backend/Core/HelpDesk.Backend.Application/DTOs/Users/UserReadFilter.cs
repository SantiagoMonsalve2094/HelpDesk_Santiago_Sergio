using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Application.DTOs.Users;

public sealed record UserReadFilter(
    UserRole? Role,
    Guid? SupportCategoryId,
    bool? IsActive);
