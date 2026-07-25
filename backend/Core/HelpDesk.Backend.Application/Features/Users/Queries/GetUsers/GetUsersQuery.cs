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

public sealed record GetUsersQuery(
    Guid ActorUserId,
    UserRole? Role = null,
    Guid? SupportCategoryId = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResponse<UserSummaryResponse>>;
