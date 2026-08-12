using System.Text.Json;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnglishLearning.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");
        var context = sp.GetRequiredService<EnglishLearningDbContext>();
        var environment = sp.GetService<IHostEnvironment>() ?? sp.GetService<IWebHostEnvironment>() as IHostEnvironment;

        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await context.Database.EnsureCreatedAsync(cancellationToken);
            logger.LogInformation("Non-relational database provider detected; skipped MigrateAsync.");
        }

        if (await context.Words.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Database already contains words; skipping seed.");
            return;
        }

        var seedDirectory = ResolveSeedDirectory(environment);
        if (seedDirectory is null)
        {
            logger.LogWarning("Seed directory not found. Skipping word seed.");
            return;
        }

        var wordsPath = Path.Combine(seedDirectory, "words.json");
        var wordSetsPath = Path.Combine(seedDirectory, "word-sets.json");

        if (!File.Exists(wordsPath))
        {
            logger.LogWarning("Seed file not found at {Path}. Skipping word seed.", wordsPath);
            return;
        }

        await using var wordsStream = File.OpenRead(wordsPath);
        var seedWords = await JsonSerializer.DeserializeAsync<List<SeedWordDto>>(wordsStream, JsonOptions, cancellationToken)
                        ?? [];

        var words = new List<Word>();
        foreach (var item in seedWords)
        {
            if (string.IsNullOrWhiteSpace(item.Word) ||
                string.IsNullOrWhiteSpace(item.Translation) ||
                string.IsNullOrWhiteSpace(item.Definition) ||
                string.IsNullOrWhiteSpace(item.PartOfSpeech) ||
                !Enum.TryParse<DifficultyLevel>(item.DifficultyLevel, ignoreCase: true, out var level))
            {
                continue;
            }

            words.Add(Word.Create(
                item.Word,
                item.PartOfSpeech,
                item.Definition,
                item.Translation,
                level,
                pronunciation: null,
                item.Phonetic,
                item.ExampleSentence));
        }

        if (words.Count == 0)
        {
            logger.LogWarning("No valid words found in seed file.");
            return;
        }

        await context.Words.AddRangeAsync(words, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} words.", words.Count);

        if (!File.Exists(wordSetsPath))
        {
            logger.LogWarning("Seed file not found at {Path}. Skipping word-set seed.", wordSetsPath);
            return;
        }

        await using var setsStream = File.OpenRead(wordSetsPath);
        var seedSets = await JsonSerializer.DeserializeAsync<List<SeedWordSetDto>>(setsStream, JsonOptions, cancellationToken)
                       ?? [];

        var wordLookup = words.ToDictionary(w => w.NormalizedText, w => w, StringComparer.Ordinal);

        foreach (var setDto in seedSets)
        {
            if (string.IsNullOrWhiteSpace(setDto.Name))
            {
                continue;
            }

            var wordSet = WordSet.Create(
                setDto.Name,
                setDto.Description ?? string.Empty,
                setDto.Level ?? "A1",
                setDto.Category ?? "General");

            var order = 0;
            foreach (var text in setDto.Words ?? [])
            {
                var normalized = Word.Normalize(text);
                if (!wordLookup.TryGetValue(normalized, out var word))
                {
                    continue;
                }

                wordSet.AddItem(word.Id, order++);
            }

            await context.WordSets.AddAsync(wordSet, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} word sets.", seedSets.Count);
    }

    private static string? ResolveSeedDirectory(IHostEnvironment? environment)
    {
        var candidates = new List<string>();

        if (environment is not null)
        {
            candidates.Add(Path.Combine(environment.ContentRootPath, "data", "seed"));
            candidates.Add(Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "..", "data", "seed")));
            candidates.Add(Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "data", "seed")));
        }

        candidates.Add(Path.Combine(AppContext.BaseDirectory, "data", "seed"));
        candidates.Add(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data", "seed")));
        candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), "data", "seed"));
        candidates.Add(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "data", "seed")));
        candidates.Add(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "data", "seed")));

        // Walk up from cwd looking for data/seed
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir is not null)
        {
            candidates.Add(Path.Combine(dir.FullName, "data", "seed"));
            dir = dir.Parent;
        }

        return candidates
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(Directory.Exists);
    }

    private sealed class SeedWordDto
    {
        public string? Word { get; set; }
        public string? Translation { get; set; }
        public string? Definition { get; set; }
        public string? PartOfSpeech { get; set; }
        public string? Phonetic { get; set; }
        public string? ExampleSentence { get; set; }
        public string? DifficultyLevel { get; set; }
    }

    private sealed class SeedWordSetDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Level { get; set; }
        public string? Category { get; set; }
        public List<string>? Words { get; set; }
    }
}
