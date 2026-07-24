using HelpDesk.Backend.Application.Interfaces.Queries;
using HelpDesk.Backend.Application.DTOs.Common;
using HelpDesk.Backend.Application.DTOs.Sla;
using HelpDesk.Backend.Application.DTOs.Tickets;
using HelpDesk.Backend.Application.Features.Tickets;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;
using HelpDesk.Backend.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Backend.Infrastructure.Persistence.Repositories;

internal sealed class TicketReadRepository(HelpDeskDbContext dbContext)
    : ITicketReadRepository
{
    public async Task<PagedResponse<TicketSummaryResponse>> GetPagedAsync(
        TicketReadFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = ApplyVisibility(dbContext.Tickets.AsNoTracking(), filter.Visibility);

        if (filter.Status is TicketStatus status)
        {
            query = query.Where(ticket => ticket.Status == status);
        }

        if (filter.Priority is TicketPriority priority)
        {
            query = query.Where(ticket => ticket.Priority == priority);
        }

        if (filter.SupportCategoryId is Guid supportCategoryId)
        {
            query = query.Where(ticket => ticket.SupportCategoryId == supportCategoryId);
        }

        if (filter.TechnicianUserId is Guid technicianUserId)
        {
            query = query.Where(
                ticket => ticket.CurrentTechnicianUserId == technicianUserId);
        }

        if (filter.IsOverdue is bool isOverdue)
        {
            query = query.Where(ticket =>
                (ticket.SlaCycles
                    .OrderByDescending(cycle => cycle.StartedAtUtc)
                    .Select(cycle => cycle.Outcome)
                    .First() == SlaOutcome.Breached) == isOverdue);
        }

        if (filter.CreatedFromUtc is DateTimeOffset createdFromUtc)
        {
            query = query.Where(ticket => ticket.CreatedAtUtc >= createdFromUtc);
        }

        if (filter.CreatedToUtc is DateTimeOffset createdToUtc)
        {
            query = query.Where(ticket => ticket.CreatedAtUtc <= createdToUtc);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(ticket => ticket.CreatedAtUtc)
            .ThenBy(ticket => ticket.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(ticket => new TicketSummaryRow(
                ticket.Id,
                ticket.Number,
                ticket.Subject,
                ticket.CreatorUserId,
                ticket.SupportCategoryId,
                ticket.Priority,
                ticket.Status,
                ticket.CurrentTechnicianUserId,
                ticket.SlaCycles
                    .OrderByDescending(cycle => cycle.StartedAtUtc)
                    .Select(cycle => cycle.Outcome)
                    .First() == SlaOutcome.Breached,
                ticket.SlaCycles
                    .OrderByDescending(cycle => cycle.StartedAtUtc)
                    .Select(cycle => cycle.DeadlineAtUtc)
                    .First(),
                ticket.CreatedAtUtc,
                ticket.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new TicketSummaryResponse(
                row.Id,
                row.Number.Value,
                row.Subject,
                row.CreatorUserId,
                row.SupportCategoryId,
                row.Priority,
                row.Status,
                row.CurrentTechnicianUserId,
                row.IsOverdue,
                row.SlaDeadlineAtUtc,
                row.CreatedAtUtc,
                row.UpdatedAtUtc))
            .ToArray();

        return new PagedResponse<TicketSummaryResponse>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    public async Task<IReadOnlyList<AssignableTechnicianResponse>>
        GetAssignableTechniciansAsync(
            Guid supportCategoryId,
            CancellationToken cancellationToken)
    {
        var rows = await dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.IsActive &&
                user.Role == UserRole.Technician &&
                user.TechnicianProfile != null &&
                user.TechnicianProfile.CategoryAssignments.Any(
                    assignment =>
                        assignment.SupportCategoryId == supportCategoryId))
            .Select(user => new AssignableTechnicianRow(
                user.Id,
                user.FullName,
                user.TechnicianProfile!.MaxActiveTickets,
                dbContext.Tickets.Count(ticket =>
                    ticket.CurrentTechnicianUserId == user.Id &&
                    (ticket.Status == TicketStatus.Assigned ||
                     ticket.Status == TicketStatus.InProgress ||
                     ticket.Status == TicketStatus.Reopened))))
            .ToListAsync(cancellationToken);

        return rows
            .Where(row => row.ActiveTicketCount < row.MaxActiveTickets)
            .OrderBy(row => row.ActiveTicketCount)
            .ThenBy(row => row.FullName)
            .Select(row => new AssignableTechnicianResponse(
                row.TechnicianUserId,
                row.FullName,
                row.MaxActiveTickets,
                row.ActiveTicketCount,
                row.MaxActiveTickets - row.ActiveTicketCount))
            .ToArray();
    }

    public async Task<PagedResponse<SlaAlertResponse>> GetSlaAlertsAsync(
        TicketVisibilityScope visibility,
        Guid? supportCategoryId,
        DateTimeOffset asOfUtc,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = ApplyVisibility(dbContext.Tickets.AsNoTracking(), visibility);
        if (supportCategoryId is Guid categoryId)
        {
            query = query.Where(ticket => ticket.SupportCategoryId == categoryId);
        }

        query = query.Where(ticket =>
            ticket.SlaCycles
                .OrderByDescending(cycle => cycle.StartedAtUtc)
                .Select(cycle => cycle.Outcome)
                .First() == SlaOutcome.Pending ||
            ticket.SlaCycles
                .OrderByDescending(cycle => cycle.StartedAtUtc)
                .Select(cycle => cycle.Outcome)
                .First() == SlaOutcome.Breached);

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(ticket =>
                ticket.SlaCycles
                    .OrderByDescending(cycle => cycle.StartedAtUtc)
                    .Select(cycle => cycle.Outcome)
                    .First() == SlaOutcome.Breached ||
                ticket.SlaCycles
                    .OrderByDescending(cycle => cycle.StartedAtUtc)
                    .Select(cycle => cycle.DeadlineAtUtc)
                    .First() < asOfUtc)
            .ThenBy(ticket =>
                ticket.SlaCycles
                    .OrderByDescending(cycle => cycle.StartedAtUtc)
                    .Select(cycle => cycle.DeadlineAtUtc)
                    .First())
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(ticket => new SlaAlertRow(
                ticket.Id,
                ticket.Number,
                ticket.Subject,
                ticket.SupportCategoryId,
                ticket.CurrentTechnicianUserId,
                ticket.Priority,
                ticket.Status,
                ticket.SlaCycles
                    .OrderByDescending(cycle => cycle.StartedAtUtc)
                    .Select(cycle => cycle.DeadlineAtUtc)
                    .First(),
                ticket.SlaCycles
                    .OrderByDescending(cycle => cycle.StartedAtUtc)
                    .Select(cycle => cycle.Outcome)
                    .First()))
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new SlaAlertResponse(
                row.TicketId,
                row.Number.Value,
                row.Subject,
                row.SupportCategoryId,
                row.CurrentTechnicianUserId,
                row.Priority,
                row.Status,
                row.DeadlineAtUtc,
                row.DeadlineAtUtc - asOfUtc,
                row.Outcome == SlaOutcome.Breached ||
                row.DeadlineAtUtc < asOfUtc))
            .ToArray();

        return new PagedResponse<SlaAlertResponse>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }

    private static IQueryable<Ticket> ApplyVisibility(
        IQueryable<Ticket> query,
        TicketVisibilityScope visibility)
    {
        return visibility.ActorRole switch
        {
            UserRole.SuperAdmin => query,
            UserRole.Supervisor when
                visibility.SupervisorSupportCategoryId is Guid categoryId =>
                query.Where(ticket => ticket.SupportCategoryId == categoryId),
            UserRole.Technician => query.Where(ticket =>
                ticket.CreatorUserId == visibility.ActorUserId ||
                ticket.CurrentTechnicianUserId == visibility.ActorUserId),
            UserRole.User => query.Where(
                ticket => ticket.CreatorUserId == visibility.ActorUserId),
            _ => query.Where(_ => false)
        };
    }

    private sealed record TicketSummaryRow(
        Guid Id,
        TicketNumber Number,
        string Subject,
        Guid CreatorUserId,
        Guid SupportCategoryId,
        TicketPriority Priority,
        TicketStatus Status,
        Guid? CurrentTechnicianUserId,
        bool IsOverdue,
        DateTimeOffset SlaDeadlineAtUtc,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);

    private sealed record AssignableTechnicianRow(
        Guid TechnicianUserId,
        string FullName,
        int MaxActiveTickets,
        int ActiveTicketCount);

    private sealed record SlaAlertRow(
        Guid TicketId,
        TicketNumber Number,
        string Subject,
        Guid SupportCategoryId,
        Guid? CurrentTechnicianUserId,
        TicketPriority Priority,
        TicketStatus Status,
        DateTimeOffset DeadlineAtUtc,
        SlaOutcome Outcome);
}
