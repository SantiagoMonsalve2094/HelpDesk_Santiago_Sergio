using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Auth;
using HelpDesk.Backend.Application.Features.Auth;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery(
    Guid ActorUserId) : IRequest<AuthenticatedUserResponse>;
