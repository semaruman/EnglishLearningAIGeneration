using EnglishLearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishLearning.Infrastructure.Persistence.Configurations;

public class UserWordConfiguration : IEntityTypeConfiguration<UserWord>
{
    public void Configure(EntityTypeBuilder<UserWord> builder)
    {
        builder.ToTable("UserWords");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(x => new { x.UserId, x.WordId })
            .IsUnique();

        builder.HasIndex(x => x.NextReviewAt);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(x => x.Word)
            .WithMany()
            .HasForeignKey(x => x.WordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.KnowledgeLevel).IsRequired();
        builder.Property(x => x.AddedAt).IsRequired();
        builder.Property(x => x.CorrectAnswers).IsRequired();
        builder.Property(x => x.IncorrectAnswers).IsRequired();
        builder.Property(x => x.ReviewCount).IsRequired();
    }
}
