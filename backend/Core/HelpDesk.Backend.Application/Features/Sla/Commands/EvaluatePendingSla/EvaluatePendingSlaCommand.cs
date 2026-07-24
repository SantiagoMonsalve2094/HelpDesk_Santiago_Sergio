using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Sla.Commands.EvaluatePendingSla;

public sealed record EvaluatePendingSlaCommand(int BatchSize = 100) : IRequest<int>;
