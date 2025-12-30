using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class FamilyUserConfiguration : IEntityTypeConfiguration<FamilyUser>
{
    public void Configure(EntityTypeBuilder<FamilyUser> builder)
    {
        builder.ToTable("FamilyUsers");

        builder.HasKey(fu => fu.Id);

        builder.Property(fu => fu.FamilyId)
            .IsRequired();

        builder.Property(fu => fu.UserId)
            .IsRequired();

        builder.Property(fu => fu.IsParent)
            .IsRequired();

        builder.Property(fu => fu.InvitedById)
            .IsRequired();

        builder.Property(fu => fu.JoinedAtUtc)
            .IsRequired();

        // Unique constraint: user can only be in a family once
        builder.HasIndex(fu => new { fu.FamilyId, fu.UserId })
            .IsUnique();

        builder.HasIndex(fu => fu.UserId);
        builder.HasIndex(fu => fu.FamilyId);

        // Relationships
        builder.HasOne(fu => fu.Family)
            .WithMany()
            .HasForeignKey(fu => fu.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fu => fu.User)
            .WithMany()
            .HasForeignKey(fu => fu.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Inviter relationship
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(fu => fu.InvitedById)
            .OnDelete(DeleteBehavior.NoAction);
    }
}