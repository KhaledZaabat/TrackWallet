using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.UserName)
            .IsUnique();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.BirthDate)
            .IsRequired(false);

        builder.Property(u => u.IsMale)
            .IsRequired(false);

        // Profile Image relationship
        builder.HasOne(u => u.ProfileImage)
            .WithMany()
            .HasForeignKey(u => u.ProfileImageFileId)
            .OnDelete(DeleteBehavior.SetNull);


        builder.HasMany(u => u.Transactions)
            .WithOne(t => t.CreatedBy)
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Audit properties
        builder.Property(u => u.CreatedAtUtc)
            .IsRequired();

        builder.Property(u => u.CreatedBy)
            .IsRequired();

        builder.Property(u => u.LastModifiedUtc)
            .IsRequired();

        builder.Property(u => u.LastModifiedBy)
            .IsRequired();

        // Soft delete
        builder.HasQueryFilter(u => !u.IsDeleted);
        // Notifications relationship
        builder.HasMany(u => u.Notifications)
            .WithOne()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(u => u.IsDeleted);
    }
}
