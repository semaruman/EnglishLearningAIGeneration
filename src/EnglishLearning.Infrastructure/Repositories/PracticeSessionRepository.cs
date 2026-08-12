using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Repositories;

public class PracticeSessionRepository : IPracticeSessionRepository
{
    private readonly EnglishLearningDbContext _context;

    public PracticeSessionRepository(EnglishLearningDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PracticeSession session, CancellationToken cancellationToken = default) =>
        await _context.PracticeSessions.AddAsync(session, cancellationToken);

    public async Task<(IReadOnlyList<PracticeSession> Items, int TotalCount)> GetByUserAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PracticeSessions.Where(x => x.UserId == userId);
        var total = await query.CountAsync(cancellationToken);
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize < 1 ? 20 : pageSize;

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task<int> CountByUserAsync(string userId, CancellationToken cancellationToken = default) =>
        _context.PracticeSessions.CountAsync(x => x.UserId == userId, cancellationToken);
}
