using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Expense_Tracker.Domain.Users.Abstraction;
using Expense_Tracker.Domain.Users.StudentsFolder;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");


        builder.HasBaseType<User>();

        builder.Property(x => x.AcademicYearId)
               .HasColumnType("uuid")
               .IsRequired();

        builder.HasOne(x => x.AcademicYear)
               .WithMany()
               .HasForeignKey(x => x.AcademicYearId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.AcademicYear)
               .UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}