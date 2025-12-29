using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Expense_Tracker.Domain.Users.Abstraction;
using Expense_Tracker.Domain.Users.AdminFolder;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.ToTable("Admins");

        // REQUIRED for TPC
        builder.HasBaseType<User>();

        builder.Property(x => x.FullName)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.Email)
               .HasMaxLength(256)
               .IsRequired();
    }
}