using Expense_Tracker.Infrastructure.Idenitity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("AspNetUsers");


        builder.Property(u => u.IsDeleted)
            .IsRequired();

        builder.Property(u => u.DeletedById)
            .IsRequired(false);

        builder.Property(u => u.DeletedOn)
            .IsRequired(false);

        builder.HasQueryFilter(u => !u.IsDeleted);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);



        builder.HasIndex(u => u.Email);
        builder.HasIndex(u => u.NormalizedEmail).IsUnique();
        builder.HasIndex(u => u.NormalizedUserName).IsUnique();
    }
}


