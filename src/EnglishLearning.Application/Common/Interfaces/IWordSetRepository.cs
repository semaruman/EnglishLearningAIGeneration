using EnglishLearning.Domain.Entities;

namespace EnglishLearning.Application.Common.Interfaces;

public interface IWordSetRepository
{
    Task<IReadOnlyList<WordSet>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<WordSet?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(WordSet wordSet, CancellationToken cancellationToken = default);
}
