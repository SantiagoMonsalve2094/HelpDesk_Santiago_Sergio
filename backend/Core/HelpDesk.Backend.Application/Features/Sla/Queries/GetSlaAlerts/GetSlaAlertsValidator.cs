using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.Common.Validation;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Sla.Queries.GetSlaAlerts;

public sealed class GetSlaAlertsValidator : AbstractValidator<GetSlaAlertsQuery>
{
    public GetSlaAlertsValidator()
    {
        RuleFor(query => query.ActorUserId).NotEmpty();
        this.ApplyPaginationRules(query => query.PageNumber, query => query.PageSize);
    }
}
