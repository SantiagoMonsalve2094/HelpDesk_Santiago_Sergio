using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.Resources;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Sla.Queries.GetSlaReport;

public sealed class GetSlaReportValidator : AbstractValidator<GetSlaReportQuery>
{
    public GetSlaReportValidator()
    {
        RuleFor(query => query.ActorUserId).NotEmpty();
        RuleFor(query => query)
            .Must(query =>
                query.FromUtc is null ||
                query.ToUtc is null ||
                query.FromUtc <= query.ToUtc)
            .WithMessage(ApplicationMessages.DateRangeInvalid);
    }
}
