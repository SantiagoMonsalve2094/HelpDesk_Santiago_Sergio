using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;

namespace HelpDesk.Backend.Application.DTOs.Sla;

public sealed record SlaComplianceGroupResponse(
    Guid SupportCategoryId,
    string SupportCategoryName,
    Guid? TechnicianUserId,
    string TechnicianName,
    int MetCycles,
    int BreachedCycles,
    int PendingCycles,
    int EvaluatedCycles,
    decimal? CompliancePercentage);
