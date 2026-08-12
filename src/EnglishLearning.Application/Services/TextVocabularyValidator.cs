using EnglishLearning.Application.Common.Interfaces;
using EnglishLearning.Application.Common.Models;

namespace EnglishLearning.Application.Services;

public class TextVocabularyValidator : ITextVocabularyValidator
{
    private readonly IWordNormalizer _normalizer;

    public TextVocabularyValidator(IWordNormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    public TextValidationResult Validate(string text, IReadOnlyCollection<string> allowedWords)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return TextValidationResult.Invalid(["(empty text)"]);
        }

        var allowedSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var word in allowedWords)
        {
            foreach (var form in _normalizer.ExpandInflections(word))
            {
                allowedSet.Add(form);
            }

            var normalized = _normalizer.Normalize(word);
            if (!string.IsNullOrEmpty(normalized))
            {
                allowedSet.Add(normalized);
            }
        }

        var tokens = _normalizer.Tokenize(text);
        var disallowed = tokens
            .Where(token => !allowedSet.Contains(token) && !allowedSet.Contains(_normalizer.Normalize(token)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return disallowed.Count == 0
            ? TextValidationResult.Valid()
            : TextValidationResult.Invalid(disallowed);
    }
}
