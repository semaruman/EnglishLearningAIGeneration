using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Domain.Entities;

public class Word
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string WordText { get; private set; } = string.Empty;
    public string NormalizedText { get; private set; } = string.Empty;
    public string PartOfSpeech { get; private set; } = string.Empty;
    public string Definition { get; private set; } = string.Empty;
    public string Translation { get; private set; } = string.Empty;
    public string? Pronunciation { get; private set; }
    public string? Phonetic { get; private set; }
    public string? ExampleSentence { get; private set; }
    public DifficultyLevel DifficultyLevel { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Word()
    {
    }

    public static Word Create(
        string wordText,
        string partOfSpeech,
        string definition,
        string translation,
        DifficultyLevel difficultyLevel,
        string? pronunciation = null,
        string? phonetic = null,
        string? exampleSentence = null)
    {
        var normalized = Normalize(wordText);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Word text is required.", nameof(wordText));
        }

        return new Word
        {
            WordText = wordText.Trim(),
            NormalizedText = normalized,
            PartOfSpeech = partOfSpeech.Trim(),
            Definition = definition.Trim(),
            Translation = translation.Trim(),
            Pronunciation = pronunciation?.Trim(),
            Phonetic = phonetic?.Trim(),
            ExampleSentence = exampleSentence?.Trim(),
            DifficultyLevel = difficultyLevel,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(
        string partOfSpeech,
        string definition,
        string translation,
        DifficultyLevel difficultyLevel,
        string? pronunciation,
        string? phonetic,
        string? exampleSentence)
    {
        PartOfSpeech = partOfSpeech.Trim();
        Definition = definition.Trim();
        Translation = translation.Trim();
        DifficultyLevel = difficultyLevel;
        Pronunciation = pronunciation?.Trim();
        Phonetic = phonetic?.Trim();
        ExampleSentence = exampleSentence?.Trim();
    }

    public static string Normalize(string text) =>
        text.Trim().ToLowerInvariant();
}
