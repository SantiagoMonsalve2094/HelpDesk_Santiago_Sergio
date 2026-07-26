using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;

namespace HelpDesk.Backend.Application.DTOs.Tickets;

public sealed record TicketCommentResponse(
    Guid Id,
    Guid AuthorUserId,
    string AuthorName,
    TicketCommentType Type,
    string Body,
    bool SatisfiesResolutionRequirement,
    DateTimeOffset CreatedAtUtc);
