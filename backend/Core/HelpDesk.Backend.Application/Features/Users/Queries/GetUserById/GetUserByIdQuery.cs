using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Users;
using HelpDesk.Backend.Application.Features.Users;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid ActorUserId, Guid UserId) : IRequest<UserDetailsResponse>;
