using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;

namespace HelpDesk.Backend.Application.DTOs.Sla;

public sealed record SlaReportResponse(
    IReadOnlyList<SlaComplianceGroupResponse> Groups,
    int TotalMetCycles,
    int TotalBreachedCycles,
    int TotalPendingCycles,
    int TotalEvaluatedCycles,
    decimal? CompliancePercentage);
