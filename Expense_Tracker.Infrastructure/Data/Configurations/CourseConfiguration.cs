using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Expense_Tracker.Domain.AcademicYearFolder.CoursesFolder;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasColumnType("uuid")
               .IsRequired();

        builder.Property(x => x.Title)
               .IsRequired()
               .HasMaxLength(300);

        builder.Property(x => x.AcademicYearId)
               .HasColumnType("uuid")
               .IsRequired();

        builder.HasOne(x => x.AcademicYear)
               .WithMany(y => y.Courses)
               .HasForeignKey(x => x.AcademicYearId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
