using EnglishLearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EnglishLearning.Infrastructure.Persistence;

public class EnglishLearningDbContextFactory : IDesignTimeDbContextFactory<EnglishLearningDbContext>
{
    public EnglishLearningDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EnglishLearningDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=english_learning;Username=postgres;Password=postgres");

        return new EnglishLearningDbContext(optionsBuilder.Options);
    }
}
