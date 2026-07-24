using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.CloseResolvedTickets;

public sealed class CloseResolvedTicketsValidator : AbstractValidator<CloseResolvedTicketsCommand>
{
    public CloseResolvedTicketsValidator()
    {
        RuleFor(command => command.BatchSize).InclusiveBetween(1, 500);
    }
}
