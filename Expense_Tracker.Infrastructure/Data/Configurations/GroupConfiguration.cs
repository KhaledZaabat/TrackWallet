using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Expense_Tracker.Domain.GroupFolder;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("Groups");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasColumnType("uuid")
               .IsRequired();

        builder.Property(x => x.GroupNumber)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(x => x.AcademicYearId)
               .HasColumnType("uuid")
               .IsRequired();

        builder.HasOne(x => x.AcademicYear)
               .WithMany()
               .HasForeignKey(x => x.AcademicYearId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}