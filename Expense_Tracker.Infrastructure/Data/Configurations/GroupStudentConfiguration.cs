using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Expense_Tracker.Domain.Users.StudentsFolder;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class GroupStudentConfiguration : IEntityTypeConfiguration<GroupStudent>
{
    public void Configure(EntityTypeBuilder<GroupStudent> builder)
    {
        builder.ToTable("GroupStudents");

        builder.HasKey(x => new { x.GroupId, x.StudentId });

        builder.Property(x => x.GroupId)
               .HasColumnType("uuid")
               .IsRequired();

        builder.Property(x => x.StudentId)
               .HasColumnType("uuid")
               .IsRequired();

        builder.HasOne(x => x.Group)
               .WithMany(g => g.Students)
               .HasForeignKey(x => x.GroupId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Student)
               .WithMany()
               .HasForeignKey(x => x.StudentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}