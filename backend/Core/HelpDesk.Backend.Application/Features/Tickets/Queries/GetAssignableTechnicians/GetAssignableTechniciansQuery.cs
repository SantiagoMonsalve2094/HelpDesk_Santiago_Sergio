using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Queries.GetAssignableTechnicians;

public sealed record GetAssignableTechniciansQuery(
    Guid ActorUserId,
    Guid TicketId) : IRequest<IReadOnlyList<AssignableTechnicianResponse>>;
