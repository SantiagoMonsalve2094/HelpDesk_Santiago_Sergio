using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.ResetUserPassword;

public sealed record ResetUserPasswordCommand(
    Guid ActorUserId,
    Guid UserId,
    string Password) : IRequest;
