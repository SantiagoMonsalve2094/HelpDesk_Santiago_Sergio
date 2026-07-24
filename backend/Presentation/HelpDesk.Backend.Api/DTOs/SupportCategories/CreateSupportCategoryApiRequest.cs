namespace HelpDesk.Backend.Api.DTOs.SupportCategories;

public sealed record CreateSupportCategoryApiRequest(
    string Name,
    string Description,
    int LowSlaMinutes,
    int MediumSlaMinutes,
    int HighSlaMinutes,
    int CriticalSlaMinutes);
