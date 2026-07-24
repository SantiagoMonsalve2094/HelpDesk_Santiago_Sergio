using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Sla.Queries.GetSlaReport;

public sealed record GetSlaReportQuery(
    Guid ActorUserId,
    Guid? SupportCategoryId = null,
    Guid? TechnicianUserId = null,
    DateTimeOffset? FromUtc = null,
    DateTimeOffset? ToUtc = null) : IRequest<SlaReportResponse>;
