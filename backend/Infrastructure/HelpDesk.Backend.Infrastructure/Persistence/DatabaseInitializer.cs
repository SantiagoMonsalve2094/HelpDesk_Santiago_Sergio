using HelpDesk.Backend.Application.Interfaces;
<<<<<<< HEAD
using HelpDesk.Backend.Domain.Aggregates.SupportCategories;
using HelpDesk.Backend.Domain.Aggregates.Users;
using HelpDesk.Backend.Domain.Enums;
=======
using HelpDesk.Backend.Domain.Aggregates.Users;
>>>>>>> 60bd3aa8c163527f2e018e15a29114b99aa06847
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

<<<<<<< HEAD
        if (!await dbContext.Users.AnyAsync(cancellationToken))
        {
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
        }

        var seedSla = new Dictionary<TicketPriority, TimeSpan>
        {
            [TicketPriority.Critical] = TimeSpan.FromHours(2),
            [TicketPriority.High] = TimeSpan.FromHours(4),
            [TicketPriority.Medium] = TimeSpan.FromHours(8),
            [TicketPriority.Low] = TimeSpan.FromHours(24)
        };
        var requiredCategories = new (string Name, string Description)[]
        {
            ("hardware", "Incidentes relacionados con equipos y dispositivos."),
            ("software", "Incidentes relacionados con aplicaciones y sistemas."),
            ("red", "Incidentes relacionados con conectividad y redes."),
            ("otro", "Solicitudes que no pertenecen a otra categoría.")
        };
        var existingNames = await dbContext.SupportCategories
            .Select(category => category.Name)
            .ToListAsync(cancellationToken);
        foreach (var requiredCategory in requiredCategories)
        {
            if (existingNames.Any(name => string.Equals(
                    name, requiredCategory.Name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            await dbContext.SupportCategories.AddAsync(
                SupportCategory.Create(
                    requiredCategory.Name,
                    requiredCategory.Description,
                    seedSla,
                    clock.UtcNow),
                cancellationToken);
        }

=======
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
>>>>>>> 60bd3aa8c163527f2e018e15a29114b99aa06847
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
