using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using HelpDesk.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Backend.Infrastructure.Persistence.Repositories;

internal sealed class SlaReportReadRepository(HelpDeskDbContext dbContext)
    : ISlaReportReadRepository
{
    public async Task<SlaReportResponse> GetReportAsync(
        SlaReportFilter filter,
        CancellationToken cancellationToken)
    {
        var query =
            from cycle in dbContext.TicketSlaCycles.AsNoTracking()
            join ticket in dbContext.Tickets.AsNoTracking()
                on EF.Property<Guid>(cycle, "ticket_id") equals ticket.Id
            join category in dbContext.SupportCategories.AsNoTracking()
                on cycle.SupportCategoryId equals category.Id
            where
                (!filter.SupportCategoryId.HasValue ||
                 cycle.SupportCategoryId == filter.SupportCategoryId.Value) &&
                (!filter.FromUtc.HasValue ||
                 cycle.StartedAtUtc >= filter.FromUtc.Value) &&
                (!filter.ToUtc.HasValue ||
                 cycle.StartedAtUtc <= filter.ToUtc.Value)
            let effectiveTechnicianId =
                cycle.Outcome == SlaOutcome.Pending
                    ? ticket.CurrentTechnicianUserId
                    : cycle.ResponsibleTechnicianUserId
            where
                !filter.TechnicianUserId.HasValue ||
                effectiveTechnicianId == filter.TechnicianUserId.Value
            join technician in dbContext.Users.AsNoTracking()
                on effectiveTechnicianId equals (Guid?)technician.Id
                into technicians
            from technician in technicians.DefaultIfEmpty()
            select new SlaObservation(
                cycle.SupportCategoryId,
                category.Name,
                effectiveTechnicianId,
                technician == null
                    ? SlaReportLabels.UnassignedTechnician
                    : technician.FullName,
                cycle.Outcome,
                cycle.StartedAtUtc);

        var observations = await query.ToListAsync(cancellationToken);
        var groups = observations
            .GroupBy(observation => new
            {
                observation.SupportCategoryId,
                observation.SupportCategoryName,
                observation.TechnicianUserId,
                observation.TechnicianName
            })
            .Select(group =>
            {
                var met = group.Count(item => item.Outcome == SlaOutcome.Met);
                var breached = group.Count(
                    item => item.Outcome == SlaOutcome.Breached);
                var pending = group.Count(
                    item => item.Outcome == SlaOutcome.Pending);
                var evaluated = met + breached;

                return new SlaComplianceGroupResponse(
                    group.Key.SupportCategoryId,
                    group.Key.SupportCategoryName,
                    group.Key.TechnicianUserId,
                    group.Key.TechnicianName,
                    met,
                    breached,
                    pending,
                    evaluated,
                    CalculateCompliance(met, evaluated));
            })
            .OrderBy(group => group.SupportCategoryName)
            .ThenBy(group => group.TechnicianName)
            .ToArray();

        var totalMet = groups.Sum(group => group.MetCycles);
        var totalBreached = groups.Sum(group => group.BreachedCycles);
        var totalPending = groups.Sum(group => group.PendingCycles);
        var totalEvaluated = totalMet + totalBreached;

        return new SlaReportResponse(
            groups,
            totalMet,
            totalBreached,
            totalPending,
            totalEvaluated,
            CalculateCompliance(totalMet, totalEvaluated));
    }

    private static decimal? CalculateCompliance(int met, int evaluated) =>
        evaluated == 0
            ? null
            : decimal.Round(
                met * 100m / evaluated,
                2,
                MidpointRounding.AwayFromZero);

    private sealed record SlaObservation(
        Guid SupportCategoryId,
        string SupportCategoryName,
        Guid? TechnicianUserId,
        string TechnicianName,
        SlaOutcome Outcome,
        DateTimeOffset StartedAtUtc);
}
