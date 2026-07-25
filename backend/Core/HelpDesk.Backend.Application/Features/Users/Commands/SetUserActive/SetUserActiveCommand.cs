using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.SetUserActive;

public sealed record SetUserActiveCommand(
    Guid ActorUserId,
    Guid UserId,
    bool IsActive) : IRequest;
