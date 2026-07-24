using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Api.DTOs.Users;

public sealed record ChangeSupervisorCategoryApiRequest(Guid SupportCategoryId);
