using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.Common.Validation;
using HelpDesk.Backend.Application.DTOs.Users;
using HelpDesk.Backend.Application.Features.Users;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Queries.GetUsers;

public sealed class GetUsersValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersValidator()
    {
        RuleFor(query => query.ActorUserId).NotEmpty();
        RuleFor(query => query.Role).IsInEnum().When(query => query.Role.HasValue);
        this.ApplyPaginationRules(query => query.PageNumber, query => query.PageSize);
    }
}
