using HelpDesk.Backend.Application.Common.Models;
using HelpDesk.Backend.Application.Features.SupportCategories.Models;

namespace HelpDesk.Backend.Application.Abstractions.Queries;

public sealed record SupportCategoryReadFilter(bool IncludeInactive);

public interface ISupportCategoryReadRepository
{
    Task<PagedResponse<SupportCategorySummaryResponse>> GetPagedAsync(
        SupportCategoryReadFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}
