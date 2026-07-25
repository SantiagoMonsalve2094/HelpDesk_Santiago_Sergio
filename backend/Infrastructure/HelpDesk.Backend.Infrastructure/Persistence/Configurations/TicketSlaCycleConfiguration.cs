using HelpDesk.Backend.Domain.Aggregates.SupportCategories;
using HelpDesk.Backend.Domain.Aggregates.Tickets;
using HelpDesk.Backend.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Backend.Infrastructure.Persistence.Configurations;

internal sealed class TicketSlaCycleConfiguration
    : IEntityTypeConfiguration<TicketSlaCycle>
{
    public void Configure(EntityTypeBuilder<TicketSlaCycle> builder)
    {
        builder.ToTable(
            "ticket_sla_cycles",
            table => table.HasCheckConstraint(
                "ck_ticket_sla_cycles_duration",
                "[duration_ticks] > 0"));
        builder.HasKey(cycle => cycle.Id);
        builder.Property(cycle => cycle.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property<Guid>("ticket_id")
            .HasColumnName("ticket_id")
            .IsRequired();
        builder.Property(cycle => cycle.Trigger)
            .HasConversion<string>()
            .HasColumnName("trigger")
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(cycle => cycle.SupportCategoryId)
            .HasColumnName("support_category_id")
            .IsRequired();
        builder.Property(cycle => cycle.Priority)
            .HasConversion<string>()
            .HasColumnName("priority")
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(cycle => cycle.Duration)
            .HasConversion(
                duration => duration.Ticks,
                ticks => TimeSpan.FromTicks(ticks))
            .HasColumnName("duration_ticks")
            .HasColumnType("bigint")
            .IsRequired();
        builder.Property(cycle => cycle.StartedAtUtc)
            .HasColumnName("started_at_utc")
            .HasColumnType("datetimeoffset")
            .IsRequired();
        builder.Property(cycle => cycle.DeadlineAtUtc)
            .HasColumnName("deadline_at_utc")
            .HasColumnType("datetimeoffset")
            .IsRequired();
        builder.Property(cycle => cycle.RespondedAtUtc)
            .HasColumnName("responded_at_utc")
            .HasColumnType("datetimeoffset");
        builder.Property(cycle => cycle.BreachedAtUtc)
            .HasColumnName("breached_at_utc")
            .HasColumnType("datetimeoffset");
        builder.Property(cycle => cycle.ResponsibleTechnicianUserId)
            .HasColumnName("responsible_technician_user_id");
        builder.Property(cycle => cycle.Outcome)
            .HasConversion<string>()
            .HasColumnName("outcome")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex("ticket_id", nameof(TicketSlaCycle.StartedAtUtc))
            .HasDatabaseName("ix_ticket_sla_cycles_ticket_started_at_utc");
        builder.HasIndex(cycle => new { cycle.Outcome, cycle.DeadlineAtUtc })
            .HasDatabaseName("ix_ticket_sla_cycles_outcome_deadline_at_utc");
        builder.HasIndex(cycle => cycle.ResponsibleTechnicianUserId)
            .HasDatabaseName("ix_ticket_sla_cycles_responsible_technician_user_id");

        builder.HasOne<SupportCategory>()
            .WithMany()
            .HasForeignKey(cycle => cycle.SupportCategoryId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(cycle => cycle.ResponsibleTechnicianUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Ignore(cycle => cycle.IsPending);
    }
}
