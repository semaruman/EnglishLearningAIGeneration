using System.Text.RegularExpressions;
using EnglishLearning.Application.Common.Interfaces;

namespace EnglishLearning.Application.Services;

public partial class BasicWordNormalizer : IWordNormalizer
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with",
        "by", "from", "as", "is", "are", "was", "were", "be", "been", "being", "have", "has",
        "had", "do", "does", "did", "will", "would", "could", "should", "may", "might",
        "must", "shall", "can", "need", "dare", "ought", "used", "it", "its", "this", "that",
        "these", "those", "i", "you", "he", "she", "we", "they", "me", "him", "her", "us",
        "them", "my", "your", "his", "our", "their", "not", "no", "yes", "so", "if", "then",
        "than", "too", "very", "just", "about", "into", "over", "after", "before", "between",
        "under", "again", "further", "once", "here", "there", "when", "where", "why", "how",
        "all", "each", "few", "more", "most", "other", "some", "such", "only", "own", "same",
        "than", "too", "very", "s", "t", "don", "now"
    };

    public string Normalize(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return string.Empty;
        }

        var cleaned = PunctuationRegex().Replace(word.Trim().ToLowerInvariant(), string.Empty);
        if (string.IsNullOrEmpty(cleaned))
        {
            return string.Empty;
        }

        return Stem(cleaned);
    }

    public IReadOnlyList<string> Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return WordTokenRegex()
            .Matches(text.ToLowerInvariant())
            .Select(m => Normalize(m.Value))
            .Where(t => !string.IsNullOrWhiteSpace(t) && !StopWords.Contains(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public IReadOnlyList<string> ExpandInflections(string lemma)
    {
        var normalized = Normalize(lemma);
        if (string.IsNullOrEmpty(normalized))
        {
            return [];
        }

        var forms = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { normalized, lemma.Trim().ToLowerInvariant() };

        forms.Add(normalized + "s");
        forms.Add(normalized + "es");
        forms.Add(normalized + "ed");
        forms.Add(normalized + "ing");

        if (normalized.EndsWith('y') && normalized.Length > 1)
        {
            var stem = normalized[..^1];
            forms.Add(stem + "ies");
            forms.Add(stem + "ied");
        }

        if (normalized.EndsWith('e') && normalized.Length > 1)
        {
            forms.Add(normalized[..^1] + "ing");
        }

        return forms.Where(f => !string.IsNullOrWhiteSpace(f)).ToList();
    }

    private static string Stem(string word)
    {
        if (word.Length <= 3)
        {
            return word;
        }

        if (word.EndsWith("ies", StringComparison.Ordinal) && word.Length > 4)
        {
            return word[..^3] + "y";
        }

        if (word.EndsWith("ing", StringComparison.Ordinal) && word.Length > 5)
        {
            var stem = word[..^3];
            if (stem.Length >= 2 && stem[^1] == stem[^2])
            {
                stem = stem[..^1];
            }

            return stem;
        }

        if (word.EndsWith("ed", StringComparison.Ordinal) && word.Length > 4)
        {
            var stem = word[..^2];
            if (stem.Length >= 2 && stem[^1] == stem[^2])
            {
                stem = stem[..^1];
            }

            return stem;
        }

        if (word.EndsWith("es", StringComparison.Ordinal) && word.Length > 4)
        {
            return word[..^2];
        }

        if (word.EndsWith('s') && !word.EndsWith("ss", StringComparison.Ordinal) && word.Length > 3)
        {
            return word[..^1];
        }

        return word;
    }

    [GeneratedRegex(@"[^\p{L}\p{N}']+", RegexOptions.Compiled)]
    private static partial Regex PunctuationRegex();

    [GeneratedRegex(@"[\p{L}']+", RegexOptions.Compiled)]
    private static partial Regex WordTokenRegex();
}
