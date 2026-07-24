using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.Common.Validation;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Queries.GetTickets;

public sealed record GetTicketsQuery(
    Guid ActorUserId,
    TicketStatus? Status = null,
    TicketPriority? Priority = null,
    Guid? SupportCategoryId = null,
    Guid? TechnicianUserId = null,
    bool? IsOverdue = null,
    DateTimeOffset? CreatedFromUtc = null,
    DateTimeOffset? CreatedToUtc = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResponse<TicketSummaryResponse>>;
