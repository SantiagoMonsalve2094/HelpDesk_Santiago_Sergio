using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HelpDesk.Backend.Infrastructure.Persistence;

public sealed class DatabaseInitializer(
    HelpDeskDbContext dbContext,
    IConfiguration configuration,
    IPasswordHasher passwordHasher,
    IClock clock)
{
    public async Task InitializeAsync(
        bool applyMigrations,
        CancellationToken cancellationToken)
    {
        if (applyMigrations)
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var fullName = configuration["BootstrapAdmin:FullName"];
        var email = configuration["BootstrapAdmin:Email"];
        var password = configuration["BootstrapAdmin:Password"];
        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "La base de datos no contiene usuarios. Configure BootstrapAdmin__FullName, " +
                "BootstrapAdmin__Email y BootstrapAdmin__Password para crear el primer SuperAdmin.");
        }

        var administrator = User.CreateSuperAdmin(
            fullName,
            email,
            passwordHasher.Hash(password),
            clock.UtcNow);
        await dbContext.Users.AddAsync(administrator, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
