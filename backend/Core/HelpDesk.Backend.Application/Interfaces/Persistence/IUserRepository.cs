using HelpDesk.Backend.Domain.Aggregates.Users;

namespace HelpDesk.Backend.Application.Interfaces.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> ExistsByEmailAsync(string email, Guid? excludingUserId, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
}
