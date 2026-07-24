using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Policies;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.AssignTicket;

public sealed record AssignTicketCommand(
    Guid ActorUserId,
    Guid TicketId,
    Guid TechnicianUserId) : IRequest;
