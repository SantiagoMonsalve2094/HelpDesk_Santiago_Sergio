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

public sealed record GetSlaAlertsQuery(
    Guid ActorUserId,
    Guid? SupportCategoryId = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResponse<SlaAlertResponse>>;
