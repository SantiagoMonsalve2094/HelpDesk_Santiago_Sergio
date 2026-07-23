using HelpDesk.Backend.Domain.Tickets;
using HelpDesk.Backend.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Backend.Infrastructure.Persistence.Configurations;

internal sealed class TicketCommentConfiguration
    : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.ToTable("ticket_comments");
        builder.HasKey(comment => comment.Id);
        builder.Property(comment => comment.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property<Guid>("ticket_id")
            .HasColumnName("ticket_id")
            .IsRequired();
        builder.Property(comment => comment.AuthorUserId)
            .HasColumnName("author_user_id")
            .IsRequired();
        builder.Property(comment => comment.Type)
            .HasConversion<string>()
            .HasColumnName("type")
            .HasMaxLength(40)
            .IsRequired();
        builder.Property(comment => comment.Body)
            .HasColumnName("body")
            .HasMaxLength(4000)
            .IsRequired();
        builder.Property(comment => comment.SatisfiesResolutionRequirement)
            .HasColumnName("satisfies_resolution_requirement")
            .IsRequired();
        builder.Property(comment => comment.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.HasIndex("ticket_id", nameof(TicketComment.CreatedAtUtc))
            .HasDatabaseName("ix_ticket_comments_ticket_created_at_utc");
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(comment => comment.AuthorUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
