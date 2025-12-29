using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Expense_Tracker.Domain.QuizesFolder;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("Quizzes");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(q => q.Description)
            .HasMaxLength(1000);

        builder.Property(q => q.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(q => q.AvailableFromUtc)
            .IsRequired();

        builder.Property(q => q.AvailableToUtc);

        builder.Property(q => q.ShuffleQuestions)
            .IsRequired();

        builder.Property(q => q.ShowResultsImmediately)
            .IsRequired();

        builder.Property(q => q.AllowEditAfterSubmission)
            .IsRequired();

        builder.Property(q => q.ResultsReleased)
            .IsRequired();

        builder.HasOne(q => q.Course)
            .WithMany()
            .HasForeignKey(q => q.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(q => q.Questions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(q => q.Groups)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}