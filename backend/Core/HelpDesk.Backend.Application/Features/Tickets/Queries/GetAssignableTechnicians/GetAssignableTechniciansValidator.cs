using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Queries.GetAssignableTechnicians;

public sealed class GetAssignableTechniciansValidator
    : AbstractValidator<GetAssignableTechniciansQuery>
{
    public GetAssignableTechniciansValidator()
    {
        RuleFor(query => query.ActorUserId).NotEmpty();
        RuleFor(query => query.TicketId).NotEmpty();
    }
}
