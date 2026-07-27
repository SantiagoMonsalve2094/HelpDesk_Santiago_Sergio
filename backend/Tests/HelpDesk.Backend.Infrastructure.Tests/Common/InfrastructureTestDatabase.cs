using HelpDesk.Backend.Infrastructure;
using HelpDesk.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Backend.Infrastructure.Tests.Common;

internal sealed class InfrastructureTestDatabase : IAsyncDisposable
{
    private readonly ServiceProvider _serviceProvider;

    private InfrastructureTestDatabase(
        string connectionString,
        ServiceProvider serviceProvider)
    {
        ConnectionString = connectionString;
        _serviceProvider = serviceProvider;
    }

    public string ConnectionString { get; }

    public static async Task<InfrastructureTestDatabase> CreateAsync()
    {
        var databaseName = $"HelpDeskTests_{Guid.NewGuid():N}";
        var connectionString =
            $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString
            })
            .Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        var provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HelpDeskDbContext>();
        await dbContext.Database.MigrateAsync();

        return new InfrastructureTestDatabase(connectionString, provider);
    }

    public AsyncServiceScope CreateScope() =>
        _serviceProvider.CreateAsyncScope();

    public async ValueTask DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();

        var options = new DbContextOptionsBuilder<HelpDeskDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        await using var cleanupContext = new HelpDeskDbContext(options);
        await cleanupContext.Database.EnsureDeletedAsync();
    }
}
