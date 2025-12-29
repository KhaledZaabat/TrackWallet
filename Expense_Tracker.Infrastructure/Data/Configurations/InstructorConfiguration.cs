using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Expense_Tracker.Domain.Users.Abstraction;
using Expense_Tracker.Domain.Users.InstructorsFolders;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
{
    public void Configure(EntityTypeBuilder<Instructor> builder)
    {
        builder.ToTable("Instructors");

        // REQUIRED for TPC
        builder.HasBaseType<User>();

        builder.Property(i => i.Title)
               .HasMaxLength(150);

        builder.Property(i => i.Department)
               .HasMaxLength(150);

        builder.Property(i => i.PhoneNumber)
               .HasMaxLength(20);

        builder.Property(i => i.OfficeLocation)
               .HasMaxLength(200);

        builder.Navigation(i => i.Courses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(i => i.Groups)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}