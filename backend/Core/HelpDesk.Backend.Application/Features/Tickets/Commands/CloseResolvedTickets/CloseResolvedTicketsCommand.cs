using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Tickets.Commands.CloseResolvedTickets;

public sealed record CloseResolvedTicketsCommand(int BatchSize = 100) : IRequest<int>;
