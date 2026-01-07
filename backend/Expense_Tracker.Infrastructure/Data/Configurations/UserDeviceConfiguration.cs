using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class UserDeviceConfiguration
    : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        builder.ToTable("UserDevices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DeviceToken)
            .IsRequired()
            .HasMaxLength(512);

        builder.HasIndex(x => x.DeviceToken)
            .IsUnique();

        builder.Property(x => x.Platform).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedUtc).IsRequired();
        builder.Property(x => x.LastSeenUtc);

        var comparer = new ValueComparer<List<string>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList()
        );

        var property = builder.Property(x => x.SubscribedTopics)
            .HasColumnType("jsonb")
            .IsRequired();

        property.Metadata.SetValueComparer(comparer);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.IsActive });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
