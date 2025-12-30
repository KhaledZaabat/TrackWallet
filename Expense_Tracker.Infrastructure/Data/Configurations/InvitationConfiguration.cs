using Expense_Tracker.Domain.Invitation;
using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("Invitations");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.InviteeUserId)
            .IsRequired();

        builder.Property(i => i.InviterUserId)
            .IsRequired();

        builder.Property(i => i.FamilyId)
            .IsRequired();

        builder.Property(i => i.IsParent)
            .IsRequired();

        builder.Property(i => i.SentAtUtc)
            .IsRequired();

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.HasIndex(i => i.InviteeUserId);
        builder.HasIndex(i => i.InviterUserId);
        builder.HasIndex(i => i.FamilyId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => new { i.InviteeUserId, i.FamilyId, i.Status });

        // Relationships
        builder.HasOne(i => i.Family)
            .WithMany()
            .HasForeignKey(i => i.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(i => i.InviteeUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(i => i.InviterUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}