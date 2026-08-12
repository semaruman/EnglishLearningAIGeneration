using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EnglishLearning.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly object _seedLock = new();
    private bool _seeded;

    public IReadOnlyList<Guid> SeededWordIds { get; private set; } = [];

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            RemoveDbContextRegistrations(services);

            services.AddDbContext<EnglishLearningDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.RemoveAll<ILanguageModelService>();
            services.AddSingleton<ILanguageModelService, FakeLanguageModelService>();
        });
    }

    protected override void ConfigureClient(HttpClient client)
    {
        EnsureSeededWords();
        base.ConfigureClient(client);
    }

    public void EnsureSeededWords()
    {
        lock (_seedLock)
        {
            if (_seeded && SeededWordIds.Count > 0)
            {
                return;
            }

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EnglishLearningDbContext>();

            if (!db.Words.Any())
            {
                var lemmas = new[]
                {
                    "cat", "dog", "bird", "fish", "tree", "house", "book", "water",
                    "food", "friend", "school", "city", "park", "road", "apple"
                };

                foreach (var lemma in lemmas)
                {
                    db.Words.Add(Word.Create(
                        lemma,
                        "noun",
                        $"A {lemma}.",
                        lemma,
                        DifficultyLevel.A1,
                        exampleSentence: $"This is a {lemma}."));
                }

                db.SaveChanges();
            }

            SeededWordIds = db.Words
                .OrderBy(w => w.WordText)
                .Select(w => w.Id)
                .Take(15)
                .ToList();
            _seeded = true;
        }
    }

    private static void RemoveDbContextRegistrations(IServiceCollection services)
    {
        services.RemoveAll<IDbContextOptionsConfiguration<EnglishLearningDbContext>>();
        services.RemoveAll<DbContextOptions<EnglishLearningDbContext>>();
        services.RemoveAll<EnglishLearningDbContext>();
    }
}

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    public const string Name = "Integration";
}
