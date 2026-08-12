namespace EnglishLearning.Application.Common.Interfaces;

public interface IWordNormalizer
{
    string Normalize(string word);
    IReadOnlyList<string> Tokenize(string text);
    IReadOnlyList<string> ExpandInflections(string lemma);
}
