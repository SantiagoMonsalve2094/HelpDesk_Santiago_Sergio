using HelpDesk.Backend.Domain.Categories;
using HelpDesk.Backend.Domain.Tickets;
using HelpDesk.Backend.Domain.Users;
using HelpDesk.Backend.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Backend.Infrastructure.Persistence.Configurations;

internal sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");
        builder.HasKey(ticket => ticket.Id);
        builder.Property(ticket => ticket.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(ticket => ticket.Number)
            .HasConversion(
                number => number.Value,
                value => TicketNumber.Parse(value))
            .HasColumnName("ticket_number")
            .HasMaxLength(14)
            .IsRequired();
        builder.Property(ticket => ticket.Subject)
            .HasColumnName("subject")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(ticket => ticket.Description)
            .HasColumnName("description")
            .HasMaxLength(4000)
            .IsRequired();
        builder.Property(ticket => ticket.CreatorUserId)
            .HasColumnName("creator_user_id")
            .IsRequired();
        builder.Property(ticket => ticket.SupportCategoryId)
            .HasColumnName("support_category_id")
            .IsRequired();
        builder.Property(ticket => ticket.Priority)
            .HasConversion<string>()
            .HasColumnName("priority")
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(ticket => ticket.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(ticket => ticket.CurrentTechnicianUserId)
            .HasColumnName("current_technician_user_id");
        builder.Property(ticket => ticket.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();
        builder.Property(ticket => ticket.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("datetimeoffset")
            .IsRequired();
        builder.Property(ticket => ticket.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("datetimeoffset")
            .IsRequired();
        builder.Property(ticket => ticket.ResolvedAtUtc)
            .HasColumnName("resolved_at_utc")
            .HasColumnType("datetimeoffset");
        builder.Property(ticket => ticket.ClosedAtUtc)
            .HasColumnName("closed_at_utc")
            .HasColumnType("datetimeoffset");
        builder.Property<byte[]>("row_version")
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasColumnName("row_version");

        builder.HasIndex(ticket => ticket.Number)
            .IsUnique()
            .HasDatabaseName("ux_tickets_ticket_number");
        builder.HasIndex(ticket => ticket.CreatorUserId)
            .HasDatabaseName("ix_tickets_creator_user_id");
        builder.HasIndex(ticket => ticket.SupportCategoryId)
            .HasDatabaseName("ix_tickets_support_category_id");
        builder.HasIndex(ticket => ticket.CurrentTechnicianUserId)
            .HasDatabaseName("ix_tickets_current_technician_user_id");
        builder.HasIndex(ticket => new { ticket.Status, ticket.CreatedAtUtc })
            .HasDatabaseName("ix_tickets_status_created_at_utc");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(ticket => ticket.CreatorUserId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<SupportCategory>()
            .WithMany()
            .HasForeignKey(ticket => ticket.SupportCategoryId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(ticket => ticket.CurrentTechnicianUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(ticket => ticket.Assignments)
            .WithOne()
            .HasForeignKey("ticket_id")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(ticket => ticket.Assignments)
            .HasField("_assignments")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(ticket => ticket.Comments)
            .WithOne()
            .HasForeignKey("ticket_id")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(ticket => ticket.Comments)
            .HasField("_comments")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(ticket => ticket.StatusHistory)
            .WithOne()
            .HasForeignKey("ticket_id")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(ticket => ticket.StatusHistory)
            .HasField("_statusHistory")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(ticket => ticket.SlaCycles)
            .WithOne()
            .HasForeignKey("ticket_id")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(ticket => ticket.SlaCycles)
            .HasField("_slaCycles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(ticket => !ticket.IsDeleted);
        builder.Ignore(ticket => ticket.IsOverdue);
        builder.Ignore(ticket => ticket.CountsTowardTechnicianCapacity);
        builder.Ignore(ticket => ticket.CurrentSlaCycle);
        builder.Ignore(ticket => ticket.DomainEvents);
    }
}
