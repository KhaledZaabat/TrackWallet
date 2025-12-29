using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Expense_Tracker.Domain.QuizesFolder.Abstraction;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.UseTpcMappingStrategy();

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Text)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(q => q.Points)
            .IsRequired();

        builder.Property(q => q.Order)
            .IsRequired();

        builder.Property(q => q.IsTimed)
            .IsRequired();

        builder.Property(q => q.TimeLimitInMinutes);

        builder.HasOne(q => q.Quiz)
            .WithMany(qz => qz.Questions)
            .HasForeignKey(q => q.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}