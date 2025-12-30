using Expense_Tracker.Domain.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class UploadedFileConfiguration : IEntityTypeConfiguration<UploadedFile>
{
    public void Configure(EntityTypeBuilder<UploadedFile> builder)
    {
        builder.ToTable("UploadedFiles");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.EntityId)
            .IsRequired();

        builder.Property(f => f.Folder)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.FileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(f => f.StoredFileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(f => f.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.FileExtension)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(f => f.FileSizeInBytes)
            .IsRequired();


        builder.Property(f => f.IsPrimary)
            .IsRequired();

        builder.HasIndex(f => new { f.EntityType, f.EntityId });
        builder.HasIndex(f => f.StoredFileName)
            .IsUnique();
        builder.HasIndex(f => f.Folder);
    }
}