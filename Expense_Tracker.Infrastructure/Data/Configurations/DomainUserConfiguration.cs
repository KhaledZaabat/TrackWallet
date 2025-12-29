using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Expense_Tracker.Domain.Users.Abstraction;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class DomainUserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.UseTpcMappingStrategy();

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Role)
            .IsRequired();

        builder.Property(u => u.PictureUrl)
            .HasMaxLength(400);

        builder.Property(u => u.Status)
            .IsRequired();

        builder.Property(u => u.CreatedAtUtc)
            .IsRequired();

        builder.Property(u => u.LastModifiedUtc)
            .IsRequired();

        // -----------------------------
        // Soft Delete configuration
        // -----------------------------

        builder.Property(u => u.IsDeleted)
            .IsRequired();

        builder.Property(u => u.DeletedById)
            .IsRequired(false);

        builder.Property(u => u.DeletedOn)
            .IsRequired(false);

        builder.HasQueryFilter(u => !u.IsDeleted);

        // -----------------------------
        // Notification preferences
        // -----------------------------

        builder.HasOne(u => u.Notifications)
            .WithMany()
            .HasForeignKey(u => u.NotificationPreferencesId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(u => u.Notifications)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
