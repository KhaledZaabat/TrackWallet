using Expense_Tracker.Domain.Common.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

        builder.Property(x => x.TokenHash)
               .HasColumnType("bytea")
               .HasMaxLength(32)
               .IsRequired();

        builder.Property(x => x.UserId)
               .HasColumnType("uuid")
               .IsRequired();

        builder.Property(x => x.DeviceId)
               .HasMaxLength(128)
               .IsRequired();

        builder.Property(x => x.SessionFamilyId)
               .HasColumnType("uuid")
               .IsRequired();

        builder.Property(x => x.OriginalIssuedAt)
               .HasColumnType("timestamptz")
               .IsRequired();

        builder.Property(x => x.ReplacedByTokenId)
               .HasColumnType("uuid");

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.RevokedAt);

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.DeviceId });
        builder.HasIndex(x => new { x.SessionFamilyId, x.DeviceId });
        builder.HasIndex(x => new { x.ExpiresAt, x.RevokedAt });
        builder.HasIndex(x => x.ExpiresAt);
    }
}
