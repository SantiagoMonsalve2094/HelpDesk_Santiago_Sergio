using HelpDesk.Backend.Domain.Aggregates.SupportCategories;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Application.DTOs.SupportCategories;

public sealed record SlaPolicyResponse(
    Guid Id,
    TicketPriority Priority,
    TimeSpan ResponseTime);
