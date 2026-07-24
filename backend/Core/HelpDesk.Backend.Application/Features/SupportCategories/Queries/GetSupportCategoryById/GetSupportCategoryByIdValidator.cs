using FluentValidation;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Application.DTOs.SupportCategories;
using HelpDesk.Backend.Application.Features.SupportCategories;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Queries.GetSupportCategoryById;

public sealed class GetSupportCategoryByIdValidator : AbstractValidator<GetSupportCategoryByIdQuery>
{
    public GetSupportCategoryByIdValidator()
    {
        RuleFor(query => query.ActorUserId).NotEmpty();
        RuleFor(query => query.SupportCategoryId).NotEmpty();
    }
}
