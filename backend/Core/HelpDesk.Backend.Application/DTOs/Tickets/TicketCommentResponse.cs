using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Aggregates.Tickets;

namespace HelpDesk.Backend.Application.DTOs.Tickets;

public sealed record TicketCommentResponse(
    Guid Id,
    Guid AuthorUserId,
<<<<<<< HEAD
    string AuthorName,
=======
>>>>>>> 60bd3aa8c163527f2e018e15a29114b99aa06847
    TicketCommentType Type,
    string Body,
    bool SatisfiesResolutionRequirement,
    DateTimeOffset CreatedAtUtc);
