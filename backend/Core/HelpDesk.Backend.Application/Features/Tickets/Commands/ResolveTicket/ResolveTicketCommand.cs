using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.ResolveTicket;

public sealed record ResolveTicketCommand(
    Guid ActorUserId,
    Guid TicketId,
    string ResolutionComment) : IRequest;
