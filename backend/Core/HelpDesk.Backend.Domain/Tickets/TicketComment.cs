using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Domain.Tickets;

public sealed class TicketComment : Entity
{
    private const int BodyMaxLength = 4000;

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

    public Guid AuthorUserId { get; }
    public TicketCommentType Type { get; }
    public string Body { get; }
    public bool SatisfiesResolutionRequirement { get; }
    public DateTimeOffset CreatedAtUtc { get; }
}
