using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.DTOs.SupportCategories;

namespace HelpDesk.Backend.Application.Interfaces.Queries;

public interface ISupportCategoryReadRepository
{
    Task<PagedResponse<SupportCategorySummaryResponse>> GetPagedAsync(
        SupportCategoryReadFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}
