using EnglishLearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishLearning.Infrastructure.Persistence.Configurations;

public class WordSetItemConfiguration : IEntityTypeConfiguration<WordSetItem>
{
    public void Configure(EntityTypeBuilder<WordSetItem> builder)
    {
        builder.ToTable("WordSetItems");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.WordSetId, x.WordId })
            .IsUnique();

        builder.Property(x => x.Order)
            .IsRequired();

        builder.HasOne(x => x.Word)
            .WithMany()
            .HasForeignKey(x => x.WordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
