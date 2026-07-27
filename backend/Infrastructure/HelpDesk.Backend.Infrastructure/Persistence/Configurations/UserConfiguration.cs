using HelpDesk.Backend.Domain.Aggregates.SupportCategories;
using HelpDesk.Backend.Domain.Aggregates.Users;
using HelpDesk.Backend.Domain.Entities.Users;
using HelpDesk.Backend.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Backend.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(
            "users",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_users_role_profile",
                    "([role] = 'Technician' AND [technician_max_active_tickets] IS NOT NULL AND [supervisor_support_category_id] IS NULL) OR " +
                    "([role] = 'Supervisor' AND [technician_max_active_tickets] IS NULL AND [supervisor_support_category_id] IS NOT NULL) OR " +
                    "([role] IN ('User', 'SuperAdmin') AND [technician_max_active_tickets] IS NULL AND [supervisor_support_category_id] IS NULL)");
                table.HasCheckConstraint(
                    "ck_users_technician_capacity",
                    "[technician_max_active_tickets] IS NULL OR [technician_max_active_tickets] > 0");
            });

        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(user => user.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(user => user.Email)
            .HasConversion(
                email => email.Value,
                value => EmailAddress.Create(value))
            .HasColumnName("email")
            .HasMaxLength(254)
            .IsRequired();
        builder.Property(user => user.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(user => user.Role)
            .HasConversion<string>()
            .HasColumnName("role")
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(user => user.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
        builder.Property(user => user.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("datetimeoffset")
            .IsRequired();
        builder.Property(user => user.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("datetimeoffset")
            .IsRequired();
        builder.Property<byte[]>("row_version")
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasColumnName("row_version");

        builder.HasIndex(user => user.Email)
            .IsUnique()
            .HasDatabaseName("ux_users_email");
        builder.HasIndex(user => new { user.Role, user.IsActive })
            .HasDatabaseName("ix_users_role_is_active");

        builder.OwnsOne(
            user => user.TechnicianProfile,
            technician =>
            {
                technician.Property(profile => profile.MaxActiveTickets)
                    .HasColumnName("technician_max_active_tickets");

                technician.OwnsMany(
                    profile => profile.CategoryAssignments,
                    categories =>
                    {
                        categories.ToTable("technician_categories");
                        categories.WithOwner()
                            .HasForeignKey("technician_user_id");
                        categories.Property(category => category.SupportCategoryId)
                            .HasColumnName("support_category_id");
                        categories.HasKey(
                            "technician_user_id",
                            nameof(TechnicianCategory.SupportCategoryId));
                        categories.HasIndex(category => category.SupportCategoryId)
                            .HasDatabaseName("ix_technician_categories_support_category_id");
                        categories.HasOne<SupportCategory>()
                            .WithMany()
                            .HasForeignKey(category => category.SupportCategoryId)
                            .OnDelete(DeleteBehavior.NoAction);
                    });

                technician.Navigation(profile => profile.CategoryAssignments)
                    .HasField("_categoryAssignments")
                    .UsePropertyAccessMode(PropertyAccessMode.Field);
            });

        builder.OwnsOne(
            user => user.SupervisorProfile,
            supervisor =>
            {
                supervisor.Property(profile => profile.SupportCategoryId)
                    .HasColumnName("supervisor_support_category_id");
                supervisor.HasOne<SupportCategory>()
                    .WithMany()
                    .HasForeignKey(profile => profile.SupportCategoryId)
                    .OnDelete(DeleteBehavior.NoAction);
                supervisor.HasIndex(profile => profile.SupportCategoryId)
                    .HasDatabaseName("ix_users_supervisor_support_category_id");
            });

        builder.Ignore(user => user.DomainEvents);
    }
}
