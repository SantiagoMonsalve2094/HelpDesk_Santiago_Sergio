using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Policies;
using HelpDesk.Backend.Domain.Aggregates.Users;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    Guid ActorUserId,
    string FullName,
    string Email,
    string Password,
    UserRole Role,
    IReadOnlyCollection<Guid> SupportCategoryIds,
    int? MaxActiveTickets) : IRequest<Guid>;
