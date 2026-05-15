using System.Text.Json;
using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class NotificationConfiguration
    : IEntityTypeConfiguration<DomainNotification>
{
    private static readonly JsonSerializerOptions PayloadJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
    };

    public void Configure(EntityTypeBuilder<DomainNotification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.ActorUserId).IsRequired(false);

    
        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Category)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Severity)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Body).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.IconKey).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ResourceUri).HasMaxLength(512).IsRequired(false);

        // Polymorphic JSONB payload. System.Text.Json's $kind discriminator (declared
        // on NotificationPayload) handles round-tripping the concrete subtype.
        builder.Property(x => x.Payload)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null
                    ? null
                    : JsonSerializer.Serialize(v, PayloadJsonOptions),
                v => string.IsNullOrEmpty(v)
                    ? null
                    : JsonSerializer.Deserialize<NotificationPayload>(v, PayloadJsonOptions))
            .IsRequired(false);

        builder.Property(x => x.IsRead).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.ReadAtUtc).IsRequired(false);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(x => new { x.UserId, x.CreatedAtUtc })
            .HasDatabaseName("IX_Notifications_UserId_CreatedAtUtc")
            .IsDescending(false, true);

        builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAtUtc })
            .HasDatabaseName("IX_Notifications_UserId_Unread")
            .HasFilter("\"IsRead\" = false");

        builder.HasIndex(x => new { x.UserId, x.Category, x.CreatedAtUtc })
            .HasDatabaseName("IX_Notifications_UserId_Category_CreatedAtUtc");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.ActorUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
