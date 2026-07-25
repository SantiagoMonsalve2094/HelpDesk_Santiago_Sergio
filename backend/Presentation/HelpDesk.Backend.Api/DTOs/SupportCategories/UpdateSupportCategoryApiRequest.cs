namespace HelpDesk.Backend.Api.DTOs.SupportCategories;

public sealed record UpdateSupportCategoryApiRequest(
    string Name,
    string Description);
