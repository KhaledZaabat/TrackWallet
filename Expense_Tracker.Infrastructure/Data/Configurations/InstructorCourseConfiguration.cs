using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Expense_Tracker.Domain.Users.InstructorsFolders;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class InstructorCourseConfiguration : IEntityTypeConfiguration<InstructorCourse>
{
    public void Configure(EntityTypeBuilder<InstructorCourse> builder)
    {
        builder.ToTable("InstructorCourses");

        builder.HasKey(x => new { x.InstructorId, x.CourseId });

        builder.Property(x => x.InstructorId)
               .HasColumnType("uuid")
               .IsRequired();

        builder.Property(x => x.CourseId)
               .HasColumnType("uuid")
               .IsRequired();

        builder.HasOne(x => x.Instructor)
               .WithMany(i => i.Courses)
               .HasForeignKey(x => x.InstructorId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Course)
               .WithMany()
               .HasForeignKey(x => x.CourseId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}