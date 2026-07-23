using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Domain.Users;
using HelpDesk.Backend.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Backend.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(HelpDeskDbContext dbContext)
    : IUserRepository
{
    public Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.Users.SingleOrDefaultAsync(
            user => user.Id == id,
            cancellationToken);

    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailAddress.Create(email);
        return dbContext.Users.SingleOrDefaultAsync(
            user => user.Email == normalizedEmail,
            cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(
        string email,
        Guid? excludingUserId,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = EmailAddress.Create(email);
        return dbContext.Users.AnyAsync(
            user =>
                user.Email == normalizedEmail &&
                (!excludingUserId.HasValue || user.Id != excludingUserId.Value),
            cancellationToken);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return dbContext.Users.AddAsync(user, cancellationToken).AsTask();
    }
}
