using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Expense_Tracker.Domain.AcademicYearFolder;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
{
    public void Configure(EntityTypeBuilder<AcademicYear> builder)
    {
        builder.ToTable("AcademicYears");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasColumnType("uuid")
               .IsRequired();

        builder.Property(x => x.Number)
               .HasMaxLength(10)
               .IsRequired();

        builder.HasMany(x => x.Courses)
               .WithOne(c => c.AcademicYear)
               .HasForeignKey(c => c.AcademicYearId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
