using EnglishLearning.Domain.Entities;

namespace EnglishLearning.Application.Common.Interfaces;

public interface IPracticeSessionRepository
{
    Task AddAsync(PracticeSession session, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<PracticeSession> Items, int TotalCount)> GetByUserAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountByUserAsync(string userId, CancellationToken cancellationToken = default);
}
