using EnglishLearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishLearning.Infrastructure.Persistence.Configurations;

public class LearningSessionConfiguration : IEntityTypeConfiguration<LearningSession>
{
    public void Configure(EntityTypeBuilder<LearningSession> builder)
    {
        builder.ToTable("LearningSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(x => x.UserId);

        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.WordsReviewed).IsRequired();
        builder.Property(x => x.CorrectAnswers).IsRequired();
        builder.Property(x => x.IncorrectAnswers).IsRequired();
    }
}
