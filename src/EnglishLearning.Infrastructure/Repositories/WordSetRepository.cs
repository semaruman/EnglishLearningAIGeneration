using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Repositories;

public class WordSetRepository : IWordSetRepository
{
    private readonly EnglishLearningDbContext _context;

    public WordSetRepository(EnglishLearningDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WordSet>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.WordSets
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public Task<WordSet?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.WordSets
            .Include(x => x.Items)
            .ThenInclude(x => x.Word)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(WordSet wordSet, CancellationToken cancellationToken = default) =>
        await _context.WordSets.AddAsync(wordSet, cancellationToken);
}
