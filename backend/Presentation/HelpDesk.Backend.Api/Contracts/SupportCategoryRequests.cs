namespace HelpDesk.Backend.Api.Contracts;

public sealed record CreateSupportCategoryRequest(
    string Name,
    string Description,
    int LowSlaMinutes,
    int MediumSlaMinutes,
    int HighSlaMinutes,
    int CriticalSlaMinutes);

public sealed record UpdateSupportCategoryRequest(
    string Name,
    string Description);

public sealed record UpdateCategorySlaRequest(int ResponseTimeMinutes);
