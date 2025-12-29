using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Expense_Tracker.Domain.QuizesFolder.QuizOptionFolder;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable("QuestionOptions");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Text)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.IsCorrect)
            .IsRequired();

        builder.HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
