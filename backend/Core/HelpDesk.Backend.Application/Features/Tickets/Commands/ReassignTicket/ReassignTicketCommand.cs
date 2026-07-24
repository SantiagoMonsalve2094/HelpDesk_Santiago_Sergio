using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Policies;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.ReassignTicket;

public sealed record ReassignTicketCommand(
    Guid ActorUserId,
    Guid TicketId,
    Guid NewTechnicianUserId,
    string Reason) : IRequest;
