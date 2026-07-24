using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.Common.Validation;
using HelpDesk.Backend.Application.Resources;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Queries.GetTickets;

public sealed class GetTicketsValidator : AbstractValidator<GetTicketsQuery>
{
    public GetTicketsValidator()
    {
        RuleFor(query => query.ActorUserId).NotEmpty();
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
        RuleFor(query => query.Priority).IsInEnum().When(query => query.Priority.HasValue);
        RuleFor(query => query)
            .Must(query =>
                query.CreatedFromUtc is null ||
                query.CreatedToUtc is null ||
                query.CreatedFromUtc <= query.CreatedToUtc)
            .WithMessage(ApplicationMessages.DateRangeInvalid);
        this.ApplyPaginationRules(query => query.PageNumber, query => query.PageSize);
    }
}
