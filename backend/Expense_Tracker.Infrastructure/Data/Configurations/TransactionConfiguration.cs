using Expense_Tracker.Domain.TransactionFolder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(t => t.TransactedOn)
            .IsRequired();

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Notes)
            .HasMaxLength(1000);

        // Foreign key relationships
        builder.HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);



        builder.HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Audit properties
        builder.Property(t => t.CreatedAtUtc)
            .IsRequired();

        builder.Property(t => t.CreatedBy_AuditId)
            .HasColumnName("CreatedBy")
            .IsRequired();

        builder.Property(t => t.LastModifiedUtc)
            .IsRequired();

        builder.Property(t => t.LastModifiedBy)
            .IsRequired();

        // Indexes for common queries
        builder.HasIndex(t => t.FamilyId);
        builder.HasIndex(t => t.CategoryId);
        builder.HasIndex(t => t.CreatedById);
        builder.HasIndex(t => t.TransactedOn);
        builder.HasIndex(t => t.Type);
        builder.HasIndex(t => new { t.FamilyId, t.TransactedOn });
        builder.HasIndex(t => new { t.FamilyId, t.Type, t.TransactedOn });
    }
}