using HelpDesk.Backend.Domain.Aggregates.Tickets;
using HelpDesk.Backend.Domain.Aggregates.Users;
using HelpDesk.Backend.Domain.Entities.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Backend.Infrastructure.Persistence.Configurations;

internal sealed class TicketAssignmentConfiguration
    : IEntityTypeConfiguration<TicketAssignment>
{
    public void Configure(EntityTypeBuilder<TicketAssignment> builder)
    {
        builder.ToTable("ticket_assignments");
        builder.HasKey(assignment => assignment.Id);
        builder.Property(assignment => assignment.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property<Guid>("ticket_id")
            .HasColumnName("ticket_id")
            .IsRequired();
        builder.Property(assignment => assignment.TechnicianUserId)
            .HasColumnName("technician_user_id")
            .IsRequired();
        builder.Property(assignment => assignment.AssignedByUserId)
            .HasColumnName("assigned_by_user_id")
            .IsRequired();
        builder.Property(assignment => assignment.AssignedAtUtc)
            .HasColumnName("assigned_at_utc")
            .HasColumnType("datetimeoffset")
            .IsRequired();
        builder.Property(assignment => assignment.EndedAtUtc)
            .HasColumnName("ended_at_utc")
            .HasColumnType("datetimeoffset");
        builder.Property(assignment => assignment.Reason)
            .HasColumnName("reason")
            .HasMaxLength(1000);

        builder.HasIndex("ticket_id", nameof(TicketAssignment.AssignedAtUtc))
            .HasDatabaseName("ix_ticket_assignments_ticket_assigned_at_utc");
        builder.HasIndex(assignment => assignment.TechnicianUserId)
            .HasDatabaseName("ix_ticket_assignments_technician_user_id");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(assignment => assignment.TechnicianUserId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(assignment => assignment.AssignedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Ignore(assignment => assignment.IsCurrent);
    }
}
