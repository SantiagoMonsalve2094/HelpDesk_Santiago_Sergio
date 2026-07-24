using HelpDesk.Backend.Domain.Aggregates.SupportCategories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Backend.Infrastructure.Persistence.Configurations;

internal sealed class SupportCategoryConfiguration
    : IEntityTypeConfiguration<SupportCategory>
{
    public void Configure(EntityTypeBuilder<SupportCategory> builder)
    {
        builder.ToTable("support_categories");
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(category => category.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(category => category.Description)
            .HasColumnName("description")
            .HasMaxLength(1000)
            .IsRequired();
        builder.Property(category => category.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
        builder.Property(category => category.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("datetimeoffset")
            .IsRequired();
        builder.Property(category => category.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("datetimeoffset")
            .IsRequired();
        builder.Property<byte[]>("row_version")
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasColumnName("row_version");

        builder.HasIndex(category => category.Name)
            .IsUnique()
            .HasDatabaseName("ux_support_categories_name");

        builder.HasMany(category => category.SlaPolicies)
            .WithOne()
            .HasForeignKey("support_category_id")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(category => category.SlaPolicies)
            .HasField("_slaPolicies")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(category => category.DomainEvents);
    }
}
