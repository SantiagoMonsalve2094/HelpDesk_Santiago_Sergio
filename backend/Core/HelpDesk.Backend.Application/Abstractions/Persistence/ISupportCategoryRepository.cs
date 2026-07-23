using HelpDesk.Backend.Domain.Categories;

namespace HelpDesk.Backend.Application.Abstractions.Persistence;

public interface ISupportCategoryRepository
{
    Task<SupportCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, Guid? excludingCategoryId, CancellationToken cancellationToken);
    Task AddAsync(SupportCategory category, CancellationToken cancellationToken);
}
