using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Repositories;

public class WordRepository : IWordRepository
{
    private readonly EnglishLearningDbContext _context;

    public WordRepository(EnglishLearningDbContext context)
    {
        _context = context;
    }

    public Task<Word?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Words.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Word?> GetByNormalizedTextAsync(string normalizedText, CancellationToken cancellationToken = default) =>
        _context.Words.FirstOrDefaultAsync(x => x.NormalizedText == normalizedText, cancellationToken);

    public async Task<(IReadOnlyList<Word> Items, int TotalCount)> SearchAsync(
        string? query,
        string? partOfSpeech,
        DifficultyLevel? difficulty,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var words = _context.Words.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalized = Word.Normalize(query);
            words = words.Where(x =>
                x.NormalizedText.Contains(normalized) ||
                x.Translation.ToLower().Contains(normalized) ||
                x.Definition.ToLower().Contains(normalized));
        }

        if (!string.IsNullOrWhiteSpace(partOfSpeech))
        {
            var pos = partOfSpeech.Trim().ToLowerInvariant();
            words = words.Where(x => x.PartOfSpeech.ToLower() == pos);
        }

        if (difficulty.HasValue)
        {
            words = words.Where(x => x.DifficultyLevel == difficulty.Value);
        }

        var total = await words.CountAsync(cancellationToken);
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize < 1 ? 20 : pageSize;

        var items = await words
            .OrderBy(x => x.WordText)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<IReadOnlyList<Word>> GetRandomWordsNotInUserVocabularyAsync(
        string userId,
        int count,
        CancellationToken cancellationToken = default)
    {
        var userWordIds = _context.UserWords
            .Where(x => x.UserId == userId)
            .Select(x => x.WordId);

        return await _context.Words
            .Where(x => !userWordIds.Contains(x.Id))
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByNormalizedAsync(string normalizedText, CancellationToken cancellationToken = default) =>
        _context.Words.AnyAsync(x => x.NormalizedText == normalizedText, cancellationToken);

    public async Task AddAsync(Word word, CancellationToken cancellationToken = default) =>
        await _context.Words.AddAsync(word, cancellationToken);

    public async Task<IReadOnlyList<Word>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        return await _context.Words
            .Where(x => idList.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
}
