using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class NotificationConfiguration
    : IEntityTypeConfiguration<DomainNotification>
{
    public void Configure(EntityTypeBuilder<DomainNotification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ActorUserId)
            .IsRequired(false);

        builder.Property(x => x.Body)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Data)
            .HasColumnType("jsonb")  
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v)
                    ? new Dictionary<string, string>()
                    : JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null)!
            );

        builder.Property(x => x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<string>()  // Store enum as string for readability
            .HasMaxLength(50);

        // Optimized indexes for PostgreSQL

        // Basic lookup by user
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_Notifications_UserId");

        // Query unread notifications (most common query)
        builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAtUtc })
            .HasDatabaseName("IX_Notifications_UserId_IsRead_CreatedAtUtc")
            .HasFilter("\"IsRead\" = false");  // Partial index for unread only

        // Query by notification type
        builder.HasIndex(x => new { x.UserId, x.Type, x.CreatedAtUtc })
            .HasDatabaseName("IX_Notifications_UserId_Type_CreatedAtUtc");

        // Query by actor (who triggered the notification)
        builder.HasIndex(x => x.ActorUserId)
            .HasDatabaseName("IX_Notifications_ActorUserId")
            .HasFilter("\"ActorUserId\" IS NOT NULL");  // Partial index

        // GIN index for JSONB data querying (optional, if you query JSON fields)
        builder.HasIndex(x => x.Data)
            .HasDatabaseName("IX_Notifications_Data_GIN")
            .HasMethod("gin");  // Generalized Inverted Index for JSONB

        // Foreign key relationships
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.ActorUserId)
            .OnDelete(DeleteBehavior.SetNull);  // Changed to SetNull for safety
    }
}