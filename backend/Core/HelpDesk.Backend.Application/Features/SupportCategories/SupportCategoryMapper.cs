using HelpDesk.Backend.Application.DTOs.SupportCategories;
using HelpDesk.Backend.Domain.Aggregates.SupportCategories;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Application.Features.SupportCategories;

internal static class SupportCategoryMapper
{
    internal static SupportCategoryDetailsResponse ToDetails(SupportCategory category) =>
        new(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive,
            category.CreatedAtUtc,
            category.UpdatedAtUtc,
            category.SlaPolicies
                .OrderBy(policy => policy.Priority)
                .Select(policy => new SlaPolicyResponse(
                    policy.Id,
                    policy.Priority,
                    policy.ResponseTime))
                .ToArray());
}
