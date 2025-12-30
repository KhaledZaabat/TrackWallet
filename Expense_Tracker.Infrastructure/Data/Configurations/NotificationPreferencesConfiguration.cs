using Expense_Tracker.Domain.Users.Abstraction.NotificationPreferencesFolder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class NotificationPreferencesConfiguration
    : IEntityTypeConfiguration<NotificationPreferences>
{
    public void Configure(EntityTypeBuilder<NotificationPreferences> builder)
    {
        builder.ToTable("NotificationPreferences");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.EmailNotifications)
            .IsRequired();

        builder.Property(n => n.PushNotifications)
            .IsRequired();


        builder.HasData(
            // Combination 1: Email only 
            new
            {
                Id = Guid.Parse("018f3f0d-9c2c-7ab1-96da-4b821eac09ff"),
                EmailNotifications = true,
                PushNotifications = false
            },
            // Combination 2: Push only
            new
            {
                Id = Guid.Parse("018f3f0d-9c2c-7ab1-96da-4b821eac09f1"),
                EmailNotifications = false,
                PushNotifications = true
            },
            // Combination 3: Both Email and Push
            new
            {
                Id = Guid.Parse("018f3f0d-9c2c-7ab1-96da-4b821eac09f2"),
                EmailNotifications = true,
                PushNotifications = true
            },

            new
            {
                Id = Guid.Parse("018f3f0d-9c2c-7ab1-96da-4b821eac09f3"),
                EmailNotifications = false,
                PushNotifications = false
            }
        );
    }
}
