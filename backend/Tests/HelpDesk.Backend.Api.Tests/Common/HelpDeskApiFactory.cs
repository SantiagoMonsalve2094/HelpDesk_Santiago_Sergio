using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HelpDesk.Backend.Api.Tests.Common;

internal sealed class HelpDeskApiFactory(
    bool includeBootstrapConfiguration = true)
    : WebApplicationFactory<Program>
{
    internal const string AdminEmail = "santiago.monsalve@helpdesk.test";
    internal const string AdminPassword = "Admin password 2026!";
    internal const string SigningKey =
        "HelpDesk.Tests.SigningKey.With.At.Least.32.Characters";

    private readonly string _databaseName = $"HelpDeskApiTests_{Guid.NewGuid():N}";

    internal TestClock Clock { get; } = new(DateTimeOffset.UtcNow.AddMinutes(-5));

    internal string ConnectionString =>
        $"Server=(localdb)\\MSSQLLocalDB;Database={_databaseName};Trusted_Connection=True;" +
        "TrustServerCertificate=True;MultipleActiveResultSets=True";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        var values = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = ConnectionString,
            ["Jwt:Issuer"] = "HelpDesk.Api.Tests",
            ["Jwt:Audience"] = "HelpDesk.Api.Tests.Client",
            ["Jwt:SigningKey"] = SigningKey,
            ["Jwt:AccessTokenMinutes"] = "60"
        };
        if (includeBootstrapConfiguration)
        {
            values["BootstrapAdmin:FullName"] = "Santiago Monsalve";
            values["BootstrapAdmin:Email"] = AdminEmail;
            values["BootstrapAdmin:Password"] = AdminPassword;
        }

        foreach (var value in values)
        {
            builder.UseSetting(value.Key, value.Value);
        }

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IClock>();
            services.AddSingleton<IClock>(Clock);
        });
    }

    internal HttpClient CreateApiClient() =>
        CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

    internal async Task DeleteDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<HelpDeskDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        await using var context = new HelpDeskDbContext(options);
        await context.Database.EnsureDeletedAsync();
    }
}
