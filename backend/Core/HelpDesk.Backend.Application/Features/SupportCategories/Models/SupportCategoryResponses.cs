using HelpDesk.Backend.Domain.Categories;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Models;

public sealed record SlaPolicyResponse(
    Guid Id,
    TicketPriority Priority,
    TimeSpan ResponseTime);

public sealed record SupportCategorySummaryResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    DateTimeOffset UpdatedAtUtc);

public sealed record SupportCategoryDetailsResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyCollection<SlaPolicyResponse> SlaPolicies);

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
