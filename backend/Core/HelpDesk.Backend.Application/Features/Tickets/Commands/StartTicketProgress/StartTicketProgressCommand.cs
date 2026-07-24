using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.StartTicketProgress;

public sealed record StartTicketProgressCommand(Guid ActorUserId, Guid TicketId) : IRequest;
