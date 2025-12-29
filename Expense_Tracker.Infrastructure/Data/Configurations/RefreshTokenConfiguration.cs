using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Expense_Tracker.Domain.Common.Identity;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasColumnType("uuid")
               .IsRequired();

        builder.Property(x => x.Token)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(x => x.UserId)
               .HasColumnType("uuid")
               .IsRequired();

        builder.Property(x => x.DeviceId)
               .HasMaxLength(128)
               .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.RevokedAt);

        builder.HasIndex(x => x.Token).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.DeviceId });
    }
}
