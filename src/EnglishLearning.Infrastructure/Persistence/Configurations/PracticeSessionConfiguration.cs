using EnglishLearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishLearning.Infrastructure.Persistence.Configurations;

public class PracticeSessionConfiguration : IEntityTypeConfiguration<PracticeSession>
{
    public void Configure(EntityTypeBuilder<PracticeSession> builder)
    {
        builder.ToTable("PracticeSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(x => x.UserId);

        builder.Property(x => x.Prompt)
            .IsRequired();

        builder.Property(x => x.GeneratedText)
            .IsRequired();

        builder.Property(x => x.Difficulty)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Topic)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.WordCount).IsRequired();
    }
}
