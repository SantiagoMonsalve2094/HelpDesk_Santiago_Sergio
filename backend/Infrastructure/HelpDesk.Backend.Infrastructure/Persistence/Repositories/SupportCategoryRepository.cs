using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Domain.Categories;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Backend.Infrastructure.Persistence.Repositories;

internal sealed class SupportCategoryRepository(HelpDeskDbContext dbContext)
    : ISupportCategoryRepository
{
    public Task<SupportCategory?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.SupportCategories
            .Include(category => category.SlaPolicies)
            .SingleOrDefaultAsync(category => category.Id == id, cancellationToken);

    public Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludingCategoryId,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim();
        return dbContext.SupportCategories.AnyAsync(
            category =>
                category.Name == normalizedName &&
                (!excludingCategoryId.HasValue ||
                 category.Id != excludingCategoryId.Value),
            cancellationToken);
    }

    public Task AddAsync(
        SupportCategory category,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return dbContext.SupportCategories.AddAsync(category, cancellationToken).AsTask();
    }
}
