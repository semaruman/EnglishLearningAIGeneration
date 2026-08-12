using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Repositories;

public class LearningSessionRepository : ILearningSessionRepository
{
    private readonly EnglishLearningDbContext _context;

    public LearningSessionRepository(EnglishLearningDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(LearningSession session, CancellationToken cancellationToken = default) =>
        await _context.LearningSessions.AddAsync(session, cancellationToken);

    public Task<LearningSession?> GetActiveAsync(string userId, CancellationToken cancellationToken = default) =>
        _context.LearningSessions
            .Where(x => x.UserId == userId && x.CompletedAt == null)
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public Task UpdateAsync(LearningSession session, CancellationToken cancellationToken = default)
    {
        _context.LearningSessions.Update(session);
        return Task.CompletedTask;
    }

    public Task<LearningSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.LearningSessions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
