using HelpDesk.Backend.Domain.Aggregates.SupportCategories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Backend.Infrastructure.Persistence.Configurations;

internal sealed class SlaPolicyConfiguration : IEntityTypeConfiguration<SlaPolicy>
{
    public void Configure(EntityTypeBuilder<SlaPolicy> builder)
    {
        builder.ToTable(
            "sla_policies",
            table => table.HasCheckConstraint(
                "ck_sla_policies_response_time",
                "[response_time_ticks] > 0"));
        builder.HasKey(policy => policy.Id);
        builder.Property(policy => policy.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property<Guid>("support_category_id")
            .HasColumnName("support_category_id")
            .IsRequired();
        builder.Property(policy => policy.Priority)
            .HasConversion<string>()
            .HasColumnName("priority")
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(policy => policy.ResponseTime)
            .HasConversion(
                duration => duration.Ticks,
                ticks => TimeSpan.FromTicks(ticks))
            .HasColumnName("response_time_ticks")
            .HasColumnType("bigint")
            .IsRequired();

        builder.HasIndex("support_category_id", nameof(SlaPolicy.Priority))
            .IsUnique()
            .HasDatabaseName("ux_sla_policies_category_priority");
    }
}
