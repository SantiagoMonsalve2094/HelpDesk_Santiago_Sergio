using FluentValidation;
using HelpDesk.Backend.Application.Resources;
using System.Linq.Expressions;

namespace HelpDesk.Backend.Application.Common.Validation;

internal static class PaginationValidatorExtensions
{
    internal const int DefaultPageNumber = 1;
    internal const int DefaultPageSize = 20;
    internal const int MaximumPageSize = 100;

    internal static void ApplyPaginationRules<T>(
        this AbstractValidator<T> validator,
        Expression<Func<T, int>> pageNumber,
        Expression<Func<T, int>> pageSize)
    {
        validator.RuleFor(pageNumber)
            .GreaterThanOrEqualTo(DefaultPageNumber)
            .WithErrorCode(ApplicationErrorCodes.InvalidPageNumber);

        validator.RuleFor(pageSize)
            .InclusiveBetween(1, MaximumPageSize)
            .WithErrorCode(ApplicationErrorCodes.InvalidPageSize);
    }
}
