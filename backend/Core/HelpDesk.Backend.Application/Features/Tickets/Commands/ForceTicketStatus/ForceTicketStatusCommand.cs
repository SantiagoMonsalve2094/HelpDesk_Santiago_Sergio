using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.ForceTicketStatus;

public sealed record ForceTicketStatusCommand(
    Guid ActorUserId,
    Guid TicketId,
    TicketStatus TargetStatus,
    string Justification) : IRequest;
