using FluentValidation;
using HelpDesk.Backend.Application.Features.Tickets.Commands.CreateTicket;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Backend.Application.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_RegistersMediatRAndValidators()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMediator));
        Assert.Contains(
            services,
            descriptor =>
                descriptor.ServiceType == typeof(IValidator<CreateTicketCommand>));
    }
}
