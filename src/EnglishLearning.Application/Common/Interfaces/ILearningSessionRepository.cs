using EnglishLearning.Domain.Entities;

namespace EnglishLearning.Application.Common.Interfaces;

public interface ILearningSessionRepository
{
    Task AddAsync(LearningSession session, CancellationToken cancellationToken = default);

    Task<LearningSession?> GetActiveAsync(string userId, CancellationToken cancellationToken = default);

    Task UpdateAsync(LearningSession session, CancellationToken cancellationToken = default);

    Task<LearningSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
