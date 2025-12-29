using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Expense_Tracker.Infrastructure.Idenitity;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // Identity table
        builder.ToTable("AspNetUsers");

        // -----------------------------
        // Soft delete
        // -----------------------------

        builder.Property(u => u.IsDeleted)
            .IsRequired();

        builder.Property(u => u.DeletedById)
            .IsRequired(false);

        builder.Property(u => u.DeletedOn)
            .IsRequired(false);

        //  Critical: prevent deleted users from login / queries
        builder.HasQueryFilter(u => !u.IsDeleted);

        // -----------------------------
        // Refresh tokens
        // -----------------------------

        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

      

        builder.HasIndex(u => u.Email);
        builder.HasIndex(u => u.NormalizedEmail).IsUnique();
        builder.HasIndex(u => u.NormalizedUserName).IsUnique();
    }
}