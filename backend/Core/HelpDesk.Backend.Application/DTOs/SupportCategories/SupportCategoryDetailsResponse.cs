using HelpDesk.Backend.Domain.Aggregates.SupportCategories;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Application.DTOs.SupportCategories;

public sealed record SupportCategoryDetailsResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyCollection<SlaPolicyResponse> SlaPolicies);
