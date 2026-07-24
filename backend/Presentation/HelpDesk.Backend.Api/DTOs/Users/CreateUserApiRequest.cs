using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Api.DTOs.Users;

public sealed record CreateUserApiRequest(
    string FullName,
    string Email,
    string Password,
    UserRole Role,
    IReadOnlyCollection<Guid> SupportCategoryIds,
    int? MaxActiveTickets);
