using HelpDesk.Backend.Application.Common.Models;
using HelpDesk.Backend.Application.Features.Users.Models;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Application.Abstractions.Queries;

public sealed record UserReadFilter(
    UserRole? Role,
    Guid? SupportCategoryId,
    bool? IsActive);

public interface IUserReadRepository
{
    Task<PagedResponse<UserSummaryResponse>> GetPagedAsync(
        UserReadFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}
