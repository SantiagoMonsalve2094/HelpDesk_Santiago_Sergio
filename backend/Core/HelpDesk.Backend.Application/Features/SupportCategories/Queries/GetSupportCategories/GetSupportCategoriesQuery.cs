using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.Common.Validation;
using HelpDesk.Backend.Application.DTOs.SupportCategories;
using HelpDesk.Backend.Application.Features.SupportCategories;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Queries.GetSupportCategories;

public sealed record GetSupportCategoriesQuery(
    Guid ActorUserId,
    bool IncludeInactive = false,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResponse<SupportCategorySummaryResponse>>;
