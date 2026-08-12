using EnglishLearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishLearning.Infrastructure.Persistence.Configurations;

public class WordConfiguration : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        builder.ToTable("Words");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.WordText)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NormalizedText)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(x => x.NormalizedText)
            .IsUnique();

        builder.Property(x => x.PartOfSpeech)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Definition)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Translation)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Pronunciation)
            .HasMaxLength(200);

        builder.Property(x => x.Phonetic)
            .HasMaxLength(200);

        builder.Property(x => x.ExampleSentence)
            .HasMaxLength(1000);

        builder.Property(x => x.DifficultyLevel)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}
