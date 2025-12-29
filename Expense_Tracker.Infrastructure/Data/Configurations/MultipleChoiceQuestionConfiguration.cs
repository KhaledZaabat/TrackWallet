using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Expense_Tracker.Domain.QuizesFolder.QuestionsFolder;

namespace Expense_Tracker.Infrastructure.Data.Configurations;

public sealed class MultipleChoiceQuestionConfiguration : IEntityTypeConfiguration<MultipleChoiceQuestion>
{
    public void Configure(EntityTypeBuilder<MultipleChoiceQuestion> builder)
    {
        builder.ToTable("MultipleChoiceQuestions");

        builder.Property(q => q.ShuffleOptions)
            .IsRequired();

        builder.Navigation(q => q.Options)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}