using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.DTOs.Users;

namespace HelpDesk.Backend.Application.Interfaces.Queries;

public interface IUserReadRepository
{
    Task<PagedResponse<UserSummaryResponse>> GetPagedAsync(
        UserReadFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}
