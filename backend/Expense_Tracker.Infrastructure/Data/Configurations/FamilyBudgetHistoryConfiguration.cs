using Expense_Tracker.Domain.FamilyUserFolder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class FamilyBudgetHistoryConfiguration
    : IEntityTypeConfiguration<FamilyBudgetHistory>
{
    public void Configure(EntityTypeBuilder<FamilyBudgetHistory> builder)
    {
        builder.ToTable("FamilyBudgetHistories");

        builder.HasKey(fbh => fbh.Id);

        builder.Property(fbh => fbh.FamilyId)
            .IsRequired();

        builder.Property(fbh => fbh.Budget)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(fbh => fbh.RecordedAtUtc)
            .IsRequired();

        builder.HasIndex(fbh => fbh.FamilyId);
        builder.HasIndex(fbh => new { fbh.FamilyId, fbh.RecordedAtUtc });

        // Relationship
        builder.HasOne(fbh => fbh.Family)
            .WithMany()
            .HasForeignKey(fbh => fbh.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}