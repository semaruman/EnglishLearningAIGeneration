using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Common.Options;
using Microsoft.Extensions.Options;

namespace EnglishLearning.Application.Services;

public class DefaultVocabularySelectionStrategy : IVocabularySelectionStrategy
{
    private readonly IUserWordRepository _userWordRepository;
    private readonly PracticeOptions _options;

    public DefaultVocabularySelectionStrategy(
        IUserWordRepository userWordRepository,
        IOptions<PracticeOptions> options)
    {
        _userWordRepository = userWordRepository;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<string>> SelectAsync(
        string userId,
        string? difficulty,
        int maxWords,
        CancellationToken cancellationToken = default)
    {
        var limit = Math.Clamp(maxWords, 1, _options.MaxVocabularyWords);
        var words = await _userWordRepository.GetAllowedVocabularyWordsAsync(userId, cancellationToken);

        IEnumerable<string> selected = words;

        if (!string.IsNullOrWhiteSpace(difficulty))
        {
            // Difficulty filtering of lemma strings is best-effort; repository already returns vocab lemmas.
            // Keep all words; difficulty primarily guides generation length/style in the prompt.
            selected = words;
        }

        return selected
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToList();
    }
}
