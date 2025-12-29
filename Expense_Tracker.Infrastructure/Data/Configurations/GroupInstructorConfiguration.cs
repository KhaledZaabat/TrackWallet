using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Expense_Tracker.Domain.GroupFolder;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class GroupInstructorConfiguration : IEntityTypeConfiguration<GroupInstructor>
{
    public void Configure(EntityTypeBuilder<GroupInstructor> builder)
    {
        builder.ToTable("GroupInstructors");

        builder.HasKey(x => new { x.GroupId, x.InstructorId });

        builder.Property(x => x.GroupId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.InstructorId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.HasOne(x => x.Group)
            .WithMany(g => g.Instructors)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Instructor)
            .WithMany(i => i.Groups)
            .HasForeignKey(x => x.InstructorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
