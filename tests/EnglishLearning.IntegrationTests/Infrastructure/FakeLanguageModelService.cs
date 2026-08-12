using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Common.Models;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.IntegrationTests.Infrastructure;

public sealed class FakeLanguageModelService : ILanguageModelService
{
    public Task<GeneratedWordData> GenerateWordDataAsync(
        string word,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new GeneratedWordData
        {
            WordText = word,
            PartOfSpeech = "noun",
            Definition = $"A definition for {word}.",
            Translation = word,
            Pronunciation = null,
            Phonetic = null,
            ExampleSentence = $"I like {word}.",
            DifficultyLevel = DifficultyLevel.A1
        });
    }

    public Task<string> GeneratePracticeTextAsync(
        PracticeTextRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Topic.Contains("FORCE_INVALID", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult("The xyzzy jumps over something strange.");
        }

        var words = request.AllowedWords.Take(Math.Min(8, request.AllowedWords.Count)).ToList();
        if (words.Count == 0)
        {
            return Task.FromResult(string.Empty);
        }

        // Content words only — stop words are ignored by the validator.
        var text = string.Join(" ", words);
        return Task.FromResult(text);
    }
}
