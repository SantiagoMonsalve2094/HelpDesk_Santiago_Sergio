using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.DTOs.SupportCategories;
using HelpDesk.Backend.Application.Features.SupportCategories;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Backend.Infrastructure.Persistence.Repositories;

internal sealed class SupportCategoryReadRepository(HelpDeskDbContext dbContext)
    : ISupportCategoryReadRepository
{
    public async Task<PagedResponse<SupportCategorySummaryResponse>> GetPagedAsync(
        SupportCategoryReadFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.SupportCategories.AsNoTracking();
        if (!filter.IncludeInactive)
        {
            query = query.Where(category => category.IsActive);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(category => category.Name)
            .ThenBy(category => category.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(category => new SupportCategorySummaryResponse(
                category.Id,
                category.Name,
                category.Description,
                category.IsActive,
                category.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResponse<SupportCategorySummaryResponse>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }
}
