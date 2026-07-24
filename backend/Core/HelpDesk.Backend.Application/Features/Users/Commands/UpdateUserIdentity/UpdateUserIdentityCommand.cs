using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.UpdateUserIdentity;

public sealed record UpdateUserIdentityCommand(
    Guid ActorUserId,
    Guid UserId,
    string FullName,
    string Email) : IRequest;
