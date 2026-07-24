using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.ReopenTicket;

public sealed record ReopenTicketCommand(Guid ActorUserId, Guid TicketId) : IRequest;
