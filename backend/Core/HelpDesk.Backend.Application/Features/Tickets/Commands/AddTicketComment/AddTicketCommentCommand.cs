using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.AddTicketComment;

public sealed record AddTicketCommentCommand(
    Guid ActorUserId,
    Guid TicketId,
    string Body) : IRequest;
