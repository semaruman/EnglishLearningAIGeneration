using EnglishLearning.Application.Common.Models;

namespace EnglishLearning.Application.Common.Interfaces;

public interface ITextVocabularyValidator
{
    TextValidationResult Validate(string text, IReadOnlyCollection<string> allowedWords);
}
