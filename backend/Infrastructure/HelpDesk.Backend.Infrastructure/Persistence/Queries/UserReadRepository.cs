using HelpDesk.Backend.Application.Abstractions.Queries;
using HelpDesk.Backend.Application.Common.Models;
using HelpDesk.Backend.Application.Features.Users.Models;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Backend.Infrastructure.Persistence.Queries;

internal sealed class UserReadRepository(HelpDeskDbContext dbContext)
    : IUserReadRepository
{
    public async Task<PagedResponse<UserSummaryResponse>> GetPagedAsync(
        UserReadFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Users.AsNoTracking();

        if (filter.Role is UserRole role)
        {
            query = query.Where(user => user.Role == role);
        }

        if (filter.IsActive is bool isActive)
        {
            query = query.Where(user => user.IsActive == isActive);
        }

        if (filter.SupportCategoryId is Guid supportCategoryId)
        {
            query = query.Where(user =>
                (user.SupervisorProfile != null &&
                 user.SupervisorProfile.SupportCategoryId == supportCategoryId) ||
                (user.TechnicianProfile != null &&
                 user.TechnicianProfile.CategoryAssignments.Any(
                     assignment =>
                         assignment.SupportCategoryId == supportCategoryId)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(user => user.FullName)
            .ThenBy(user => user.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new UserSummaryRow(
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.IsActive))
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new UserSummaryResponse(
                row.Id,
                row.FullName,
                row.Email.Value,
                row.Role,
                row.IsActive))
            .ToArray();

        return new PagedResponse<UserSummaryResponse>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    private sealed record UserSummaryRow(
        Guid Id,
        string FullName,
        EmailAddress Email,
        UserRole Role,
        bool IsActive);
}
