using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HelpDesk.Backend.Infrastructure.Persistence;

public sealed class DesignTimeHelpDeskDbContextFactory
    : IDesignTimeDbContextFactory<HelpDeskDbContext>
{
    private const string LocalDbFallback =
        "Server=(localdb)\\MSSQLLocalDB;Database=HelpDeskMigrations;Trusted_Connection=True;TrustServerCertificate=True";

    public HelpDeskDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? LocalDbFallback;

        var options = new DbContextOptionsBuilder<HelpDeskDbContext>()
            .UseSqlServer(
                connectionString,
                sqlServer => sqlServer.MigrationsAssembly(
                    typeof(HelpDeskDbContext).Assembly.FullName))
            .Options;

        return new HelpDeskDbContext(options);
    }
}
