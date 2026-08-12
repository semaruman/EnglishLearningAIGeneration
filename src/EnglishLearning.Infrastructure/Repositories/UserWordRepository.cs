using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Repositories;

public class UserWordRepository : IUserWordRepository
{
    private readonly EnglishLearningDbContext _context;

    public UserWordRepository(EnglishLearningDbContext context)
    {
        _context = context;
    }

    public Task<UserWord?> GetByUserAndWordAsync(
        string userId,
        Guid wordId,
        CancellationToken cancellationToken = default) =>
        _context.UserWords
            .Include(x => x.Word)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.WordId == wordId, cancellationToken);

    public async Task<(IReadOnlyList<UserWord> Items, int TotalCount)> GetByUserAsync(
        string userId,
        WordStatus? status,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.UserWords
            .Include(x => x.Word)
            .Where(x => x.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = Word.Normalize(search);
            query = query.Where(x =>
                x.Word != null &&
                (x.Word.NormalizedText.Contains(normalized) ||
                 x.Word.Translation.ToLower().Contains(normalized)));
        }

        var total = await query.CountAsync(cancellationToken);
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize < 1 ? 20 : pageSize;

        var items = await query
            .OrderByDescending(x => x.AddedAt)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<IReadOnlyList<Guid>> GetUserWordIdsAsync(string userId, CancellationToken cancellationToken = default) =>
        await _context.UserWords
            .Where(x => x.UserId == userId)
            .Select(x => x.WordId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(UserWord userWord, CancellationToken cancellationToken = default) =>
        await _context.UserWords.AddAsync(userWord, cancellationToken);

    public Task RemoveAsync(UserWord userWord, CancellationToken cancellationToken = default)
    {
        _context.UserWords.Remove(userWord);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyDictionary<WordStatus, int>> CountByStatusAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var groups = await _context.UserWords
            .Where(x => x.UserId == userId)
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return groups.ToDictionary(x => x.Status, x => x.Count);
    }

    public async Task<IReadOnlyList<UserWord>> GetDueForReviewAsync(
        string userId,
        int count,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.UserWords
            .Include(x => x.Word)
            .Where(x => x.UserId == userId && x.NextReviewAt != null && x.NextReviewAt <= now)
            .OrderBy(x => x.NextReviewAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserWord>> GetWeakWordsAsync(
        string userId,
        int count,
        CancellationToken cancellationToken = default) =>
        await _context.UserWords
            .Include(x => x.Word)
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.KnowledgeLevel)
            .ThenByDescending(x => x.IncorrectAnswers)
            .Take(count)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> GetAllowedVocabularyWordsAsync(
        string userId,
        CancellationToken cancellationToken = default) =>
        await _context.UserWords
            .Where(x => x.UserId == userId)
            .Include(x => x.Word)
            .Where(x => x.Word != null)
            .Select(x => x.Word!.WordText)
            .Distinct()
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(string userId, Guid wordId, CancellationToken cancellationToken = default) =>
        _context.UserWords.AnyAsync(x => x.UserId == userId && x.WordId == wordId, cancellationToken);
}
