using HelpDesk.Backend.Domain.Aggregates.Tickets;
using HelpDesk.Backend.Domain.Aggregates.Users;
using HelpDesk.Backend.Domain.Entities.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Backend.Infrastructure.Persistence.Configurations;

internal sealed class TicketStatusChangeConfiguration
    : IEntityTypeConfiguration<TicketStatusChange>
{
    public void Configure(EntityTypeBuilder<TicketStatusChange> builder)
    {
        builder.ToTable("ticket_status_history");
        builder.HasKey(change => change.Id);
        builder.Property(change => change.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property<Guid>("ticket_id")
            .HasColumnName("ticket_id")
            .IsRequired();
        builder.Property(change => change.PreviousStatus)
            .HasConversion<string>()
            .HasColumnName("previous_status")
            .HasMaxLength(20);
        builder.Property(change => change.NewStatus)
            .HasConversion<string>()
            .HasColumnName("new_status")
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(change => change.ChangedByUserId)
            .HasColumnName("changed_by_user_id");
        builder.Property(change => change.Reason)
            .HasColumnName("reason")
            .HasMaxLength(1000);
        builder.Property(change => change.IsAutomatic)
            .HasColumnName("is_automatic")
            .IsRequired();
        builder.Property(change => change.ChangedAtUtc)
            .HasColumnName("changed_at_utc")
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.HasIndex("ticket_id", nameof(TicketStatusChange.ChangedAtUtc))
            .HasDatabaseName("ix_ticket_status_history_ticket_changed_at_utc");
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(change => change.ChangedByUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
