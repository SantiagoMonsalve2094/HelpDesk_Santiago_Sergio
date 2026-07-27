using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Domain.Entities.Tickets;

public sealed class TicketComment : Entity
{
    private const int BodyMaxLength = 4000;

    private TicketComment()
    {
        Body = string.Empty;
    }

    internal TicketComment(
        Guid id,
        Guid authorUserId,
        TicketCommentType type,
        string body,
        bool satisfiesResolutionRequirement,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        AuthorUserId = Guard.Required(
            authorUserId,
            "COMMENT_AUTHOR_REQUIRED",
            "El autor del comentario es obligatorio.");
        Type = type;
        Body = Guard.Required(
            body,
            BodyMaxLength,
            "INVALID_COMMENT",
            "El comentario es obligatorio y admite máximo 4000 caracteres.");
        SatisfiesResolutionRequirement = satisfiesResolutionRequirement;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid AuthorUserId { get; private set; }
    public TicketCommentType Type { get; private set; }
    public string Body { get; private set; }
    public bool SatisfiesResolutionRequirement { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
