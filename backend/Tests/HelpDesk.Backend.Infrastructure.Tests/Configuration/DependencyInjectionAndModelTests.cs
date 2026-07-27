using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Domain.Aggregates.SupportCategories;
using HelpDesk.Backend.Domain.Aggregates.Tickets;
using HelpDesk.Backend.Domain.Aggregates.Users;
using HelpDesk.Backend.Domain.Entities.Tickets;
using HelpDesk.Backend.Infrastructure;
using HelpDesk.Backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HelpDesk.Backend.Infrastructure.Tests;

public sealed class DependencyInjectionAndModelTests
{
    [Fact]
    public async Task AddInfrastructure_RegistersEveryApplicationPort()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=(localdb)\\MSSQLLocalDB;Database=HelpDeskDi;Trusted_Connection=True;TrustServerCertificate=True",
                ["Jwt:Issuer"] = "HelpDesk.Infrastructure.Tests",
                ["Jwt:Audience"] = "HelpDesk.Infrastructure.Tests.Client",
                ["Jwt:SigningKey"] = "Infrastructure.Tests.SigningKey.With.32.Characters",
                ["Jwt:AccessTokenMinutes"] = "60"
            })
            .Build();
        var services = new ServiceCollection();

        services.AddInfrastructure(configuration);

        await using var provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });
        await using var scope = provider.CreateAsyncScope();
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IUnitOfWork>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IUserReadRepository>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ISupportCategoryReadRepository>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ITicketReadRepository>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ISlaReportReadRepository>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IClock>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IPasswordHasher>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IAccessTokenGenerator>());
    }

    [Fact]
    public void Model_UsesExpectedTablesTypesAndConcurrencyTokens()
    {
        var options = new DbContextOptionsBuilder<HelpDeskDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=HelpDeskModel;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var context = new HelpDeskDbContext(options);

        var user = context.Model.FindEntityType(typeof(User));
        var category = context.Model.FindEntityType(typeof(SupportCategory));
        var ticket = context.Model.FindEntityType(typeof(Ticket));
        var slaCycle = context.Model.FindEntityType(typeof(TicketSlaCycle));

        Assert.Equal("users", user!.GetTableName());
        Assert.Equal("support_categories", category!.GetTableName());
        Assert.Equal("tickets", ticket!.GetTableName());
        Assert.Equal("ticket_sla_cycles", slaCycle!.GetTableName());
        Assert.True(user!.FindProperty("row_version")!.IsConcurrencyToken);
        Assert.True(category!.FindProperty("row_version")!.IsConcurrencyToken);
        Assert.True(ticket!.FindProperty("row_version")!.IsConcurrencyToken);
        Assert.Equal(
            "bigint",
            slaCycle!.FindProperty(nameof(TicketSlaCycle.Duration))!.GetColumnType());
    }

    [Fact]
    public void PasswordHasher_UsesSaltAndProducesIdentityCompatibleHashes()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=(localdb)\\MSSQLLocalDB;Database=HelpDeskHasher;Trusted_Connection=True;TrustServerCertificate=True",
                ["Jwt:Issuer"] = "HelpDesk.Infrastructure.Tests",
                ["Jwt:Audience"] = "HelpDesk.Infrastructure.Tests.Client",
                ["Jwt:SigningKey"] = "Infrastructure.Tests.SigningKey.With.32.Characters",
                ["Jwt:AccessTokenMinutes"] = "60"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();
        var hasher = provider.GetRequiredService<IPasswordHasher>();
        const string password = "Una contraseña de prueba";

        var first = hasher.Hash(password);
        var second = hasher.Hash(password);
        var identityHasher = new PasswordHasher<object>();

        Assert.NotEqual(first, second);
        Assert.DoesNotContain(password, first, StringComparison.Ordinal);
        Assert.Equal(
            PasswordVerificationResult.Success,
            identityHasher.VerifyHashedPassword(new object(), first, password));
        Assert.True(hasher.Verify(first, password));
        Assert.False(hasher.Verify(first, "contraseña incorrecta"));
    }

    [Fact]
    public void AccessTokenGenerator_EmitsRequiredClaimsAndExpiration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=(localdb)\\MSSQLLocalDB;Database=HelpDeskJwt;Trusted_Connection=True;TrustServerCertificate=True",
                ["Jwt:Issuer"] = "HelpDesk.Infrastructure.Tests",
                ["Jwt:Audience"] = "HelpDesk.Infrastructure.Tests.Client",
                ["Jwt:SigningKey"] = "Infrastructure.Tests.SigningKey.With.32.Characters",
                ["Jwt:AccessTokenMinutes"] = "60"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();
        var generator = provider.GetRequiredService<IAccessTokenGenerator>();
        var user = User.CreateSuperAdmin(
            "Santiago Monsalve",
            "admin@example.com",
            "hash",
            DateTimeOffset.UtcNow);

        var result = generator.Generate(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);

        Assert.Contains(jwt.Claims, claim =>
            claim.Type == JwtRegisteredClaimNames.Sub &&
            claim.Value == user.Id.ToString());
        Assert.Contains(jwt.Claims, claim =>
            claim.Type == "role" &&
            claim.Value == "SuperAdmin");
        Assert.True(result.ExpiresAtUtc > DateTimeOffset.UtcNow);
    }
}
