using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Policies;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.UpdateTicket;

public sealed record UpdateTicketCommand(
    Guid ActorUserId,
    Guid TicketId,
    string Subject,
    string Description) : IRequest;
