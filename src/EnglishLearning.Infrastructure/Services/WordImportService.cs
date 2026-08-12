using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using CsvHelper;
using CsvHelper.Configuration;
using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Common.Models;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Infrastructure.Services;

public class WordImportService : IWordImportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IWordRepository _wordRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WordImportService(IWordRepository wordRepository, IUnitOfWork unitOfWork)
    {
        _wordRepository = wordRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WordImportResult> ImportFromJsonAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        List<ImportWordDto>? items;
        try
        {
            items = await JsonSerializer.DeserializeAsync<List<ImportWordDto>>(stream, JsonOptions, cancellationToken);
        }
        catch (JsonException ex)
        {
            return new WordImportResult
            {
                FailedCount = 1,
                Errors = [$"Invalid JSON: {ex.Message}"]
            };
        }

        if (items is null || items.Count == 0)
        {
            return new WordImportResult { Errors = ["JSON file contains no words."] };
        }

        return await ImportAsync(items, cancellationToken);
    }

    public async Task<WordImportResult> ImportFromCsvAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        List<ImportWordDto> items;
        try
        {
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant()
            });

            items = csv.GetRecords<ImportWordDto>().ToList();
        }
        catch (Exception ex)
        {
            return new WordImportResult
            {
                FailedCount = 1,
                Errors = [$"Invalid CSV: {ex.Message}"]
            };
        }

        if (items.Count == 0)
        {
            return new WordImportResult { Errors = ["CSV file contains no words."] };
        }

        return await ImportAsync(items, cancellationToken);
    }

    private async Task<WordImportResult> ImportAsync(IReadOnlyList<ImportWordDto> items, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var imported = 0;
        var skipped = 0;
        var failed = 0;
        var seenInBatch = new HashSet<string>(StringComparer.Ordinal);

        foreach (var item in items)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(item.Word) && string.IsNullOrWhiteSpace(item.WordText))
                {
                    skipped++;
                    errors.Add("Skipped row with empty word.");
                    continue;
                }

                var wordText = (item.Word ?? item.WordText)!.Trim();
                var normalized = Word.Normalize(wordText);

                if (!seenInBatch.Add(normalized))
                {
                    skipped++;
                    continue;
                }

                if (await _wordRepository.ExistsByNormalizedAsync(normalized, cancellationToken))
                {
                    skipped++;
                    continue;
                }

                if (!TryParseDifficulty(item.DifficultyLevel, out var difficulty))
                {
                    failed++;
                    errors.Add($"Invalid difficulty for '{wordText}': '{item.DifficultyLevel}'.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Translation) ||
                    string.IsNullOrWhiteSpace(item.Definition) ||
                    string.IsNullOrWhiteSpace(item.PartOfSpeech))
                {
                    failed++;
                    errors.Add($"Missing required fields for '{wordText}'.");
                    continue;
                }

                var word = Word.Create(
                    wordText,
                    item.PartOfSpeech,
                    item.Definition,
                    item.Translation,
                    difficulty,
                    item.Pronunciation,
                    item.Phonetic,
                    item.ExampleSentence);

                await _wordRepository.AddAsync(word, cancellationToken);
                imported++;
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add(ex.Message);
            }
        }

        if (imported > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new WordImportResult
        {
            ImportedCount = imported,
            SkippedCount = skipped,
            FailedCount = failed,
            Errors = errors
        };
    }

    private static bool TryParseDifficulty(string? value, out DifficultyLevel level)
    {
        level = DifficultyLevel.A1;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return Enum.TryParse(value.Trim(), ignoreCase: true, out level);
    }

    private sealed class ImportWordDto
    {
        [JsonPropertyName("word")]
        public string? Word { get; set; }

        [JsonPropertyName("wordText")]
        public string? WordText { get; set; }

        public string? Translation { get; set; }
        public string? Definition { get; set; }
        public string? PartOfSpeech { get; set; }
        public string? Phonetic { get; set; }
        public string? Pronunciation { get; set; }
        public string? ExampleSentence { get; set; }
        public string? DifficultyLevel { get; set; }
    }
}
