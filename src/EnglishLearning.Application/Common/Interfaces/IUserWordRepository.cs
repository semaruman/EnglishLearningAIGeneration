using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;

namespace EnglishLearning.Application.Common.Interfaces;

public interface IUserWordRepository
{
    Task<UserWord?> GetByUserAndWordAsync(
        string userId,
        Guid wordId,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<UserWord> Items, int TotalCount)> GetByUserAsync(
        string userId,
        WordStatus? status,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetUserWordIdsAsync(string userId, CancellationToken cancellationToken = default);

    Task AddAsync(UserWord userWord, CancellationToken cancellationToken = default);

    Task RemoveAsync(UserWord userWord, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<WordStatus, int>> CountByStatusAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserWord>> GetDueForReviewAsync(
        string userId,
        int count,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserWord>> GetWeakWordsAsync(
        string userId,
        int count,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetAllowedVocabularyWordsAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string userId, Guid wordId, CancellationToken cancellationToken = default);
}
