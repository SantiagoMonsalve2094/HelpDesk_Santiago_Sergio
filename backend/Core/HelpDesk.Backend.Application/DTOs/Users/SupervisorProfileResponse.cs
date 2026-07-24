using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Users;

namespace HelpDesk.Backend.Application.DTOs.Users;

public sealed record SupervisorProfileResponse(Guid SupportCategoryId);
