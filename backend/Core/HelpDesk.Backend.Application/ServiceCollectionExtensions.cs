using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Backend.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}
