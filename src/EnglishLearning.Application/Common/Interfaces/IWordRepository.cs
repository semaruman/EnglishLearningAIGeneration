using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Application.Common.Interfaces;

public interface IWordRepository
{
    Task<Word?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Word?> GetByNormalizedTextAsync(string normalizedText, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Word> Items, int TotalCount)> SearchAsync(
        string? query,
        string? partOfSpeech,
        DifficultyLevel? difficulty,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Word>> GetRandomWordsNotInUserVocabularyAsync(
        string userId,
        int count,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNormalizedAsync(string normalizedText, CancellationToken cancellationToken = default);

    Task AddAsync(Word word, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Word>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
