using EnglishLearning.Domain.Entities;
using EnglishLearning.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Persistence;

public class EnglishLearningDbContext : IdentityDbContext<ApplicationUser>
{
    public EnglishLearningDbContext(DbContextOptions<EnglishLearningDbContext> options)
        : base(options)
    {
    }

    public DbSet<Word> Words => Set<Word>();
    public DbSet<UserWord> UserWords => Set<UserWord>();
    public DbSet<WordSet> WordSets => Set<WordSet>();
    public DbSet<WordSetItem> WordSetItems => Set<WordSetItem>();
    public DbSet<LearningSession> LearningSessions => Set<LearningSession>();
    public DbSet<PracticeSession> PracticeSessions => Set<PracticeSession>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(EnglishLearningDbContext).Assembly);
    }
}
