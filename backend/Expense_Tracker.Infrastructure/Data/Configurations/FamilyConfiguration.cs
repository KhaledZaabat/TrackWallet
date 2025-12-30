using Expense_Tracker.Domain.FamilyFolder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("Families");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.CurrentBudget)
            .IsRequired()
            .HasPrecision(18, 2);

        // Audit properties
        builder.Property(f => f.CreatedAtUtc)
            .IsRequired();

        builder.Property(f => f.CreatedBy)
            .IsRequired();

        builder.Property(f => f.LastModifiedUtc)
            .IsRequired();

        builder.Property(f => f.LastModifiedBy)
            .IsRequired();
        builder.HasMany(f => f.Transactions)
    .WithOne(t => t.Family)
    .HasForeignKey(t => t.FamilyId)
    .OnDelete(DeleteBehavior.Cascade);
        // Soft delete
        builder.HasQueryFilter(f => !f.IsDeleted);

        builder.HasIndex(f => f.IsDeleted);
        builder.HasIndex(f => f.CreatedBy);
        builder.HasIndex(f => f.Name);
    }
}