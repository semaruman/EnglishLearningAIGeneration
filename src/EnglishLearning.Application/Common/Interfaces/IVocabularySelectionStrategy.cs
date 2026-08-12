namespace EnglishLearning.Application.Common.Interfaces;

public interface IVocabularySelectionStrategy
{
    Task<IReadOnlyList<string>> SelectAsync(
        string userId,
        string? difficulty,
        int maxWords,
        CancellationToken cancellationToken = default);
}
