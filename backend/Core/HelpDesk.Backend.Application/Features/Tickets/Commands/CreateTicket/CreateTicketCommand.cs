using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Policies;
using HelpDesk.Backend.Domain.Aggregates.Tickets;
using HelpDesk.Backend.Domain.ValueObjects;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.CreateTicket;

public sealed record CreateTicketCommand(
    Guid ActorUserId,
    string Subject,
    string Description,
    Guid SupportCategoryId,
    TicketPriority Priority) : IRequest<CreatedTicketResponse>;
