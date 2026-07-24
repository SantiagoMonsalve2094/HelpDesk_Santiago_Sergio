using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Infrastructure.Persistence;
using HelpDesk.Backend.Infrastructure.Persistence.Repositories;
using HelpDesk.Backend.Infrastructure.Security;
using HelpDesk.Backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HelpDesk.Backend.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is required.");

        services.TryAddSingleton(configuration);
        services.AddDbContext<HelpDeskDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlServer => sqlServer.MigrationsAssembly(
                    typeof(HelpDeskDbContext).Assembly.FullName)));

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Issuer),
                "Jwt:Issuer is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Audience),
                "Jwt:Audience is required.")
            .Validate(
                options => options.SigningKey.Length >= 32,
                "Jwt:SigningKey must contain at least 32 characters.")
            .Validate(
                options => options.AccessTokenMinutes > 0,
                "Jwt:AccessTokenMinutes must be greater than zero.")
            .ValidateOnStart();

        services.AddScoped<SqlServerTicketNumberSequence>();
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<ISupportCategoryReadRepository, SupportCategoryReadRepository>();
        services.AddScoped<ITicketReadRepository, TicketReadRepository>();
        services.AddScoped<ISlaReportReadRepository, SlaReportReadRepository>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, AspNetCorePasswordHasher>();
        services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();

        return services;
    }
}
