using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Application.Common.Models;

public class GeneratedWordData
{
    public string WordText { get; init; } = string.Empty;
    public string PartOfSpeech { get; init; } = string.Empty;
    public string Definition { get; init; } = string.Empty;
    public string Translation { get; init; } = string.Empty;
    public string? Pronunciation { get; init; }
    public string? Phonetic { get; init; }
    public string? ExampleSentence { get; init; }
    public DifficultyLevel DifficultyLevel { get; init; } = DifficultyLevel.A1;
}

public class PracticeTextRequest
{
    public string Topic { get; init; } = string.Empty;
    public string Difficulty { get; init; } = "Easy";
    public string Length { get; init; } = "Short";
    public IReadOnlyList<string> AllowedWords { get; init; } = [];
}

public class TextValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> DisallowedWords { get; init; } = [];
    public string? Message { get; init; }

    public static TextValidationResult Valid() => new() { IsValid = true };

    public static TextValidationResult Invalid(IReadOnlyList<string> disallowedWords) =>
        new()
        {
            IsValid = false,
            DisallowedWords = disallowedWords,
            Message = $"Text contains words outside the allowed vocabulary: {string.Join(", ", disallowedWords)}"
        };
}

public class WordImportResult
{
    public int ImportedCount { get; init; }
    public int SkippedCount { get; init; }
    public int FailedCount { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
}
