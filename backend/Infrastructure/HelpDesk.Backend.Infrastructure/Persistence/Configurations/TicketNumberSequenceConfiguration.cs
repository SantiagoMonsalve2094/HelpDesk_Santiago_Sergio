using HelpDesk.Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Backend.Infrastructure.Persistence.Configurations;

internal sealed class TicketNumberSequenceConfiguration
    : IEntityTypeConfiguration<TicketNumberSequenceState>
{
    public void Configure(EntityTypeBuilder<TicketNumberSequenceState> builder)
    {
        builder.ToTable(
            "ticket_number_sequences",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_ticket_number_sequences_year",
                    "[year] BETWEEN 2000 AND 9999");
                table.HasCheckConstraint(
                    "ck_ticket_number_sequences_last_value",
                    "[last_value] BETWEEN 0 AND 999999");
            });
        builder.HasKey(sequence => sequence.Year);
        builder.Property(sequence => sequence.Year)
            .HasColumnName("year")
            .ValueGeneratedNever();
        builder.Property(sequence => sequence.LastValue)
            .HasColumnName("last_value")
            .IsRequired();
    }
}
