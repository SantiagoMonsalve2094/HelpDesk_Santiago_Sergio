using HelpDesk.Backend.Domain.Aggregates.SupportCategories;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Application.DTOs.SupportCategories;

public sealed record SupportCategorySummaryResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    DateTimeOffset UpdatedAtUtc);
