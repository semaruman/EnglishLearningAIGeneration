using EnglishLearning.Application.Common.Interfaces;

namespace EnglishLearning.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly EnglishLearningDbContext _context;

    public UnitOfWork(EnglishLearningDbContext context)
    {
        _context = context;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);
}
