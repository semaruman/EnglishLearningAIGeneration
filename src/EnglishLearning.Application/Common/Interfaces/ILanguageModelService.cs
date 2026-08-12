using EnglishLearning.Application.Common.Models;

namespace EnglishLearning.Application.Common.Interfaces;

public interface ILanguageModelService
{
    Task<GeneratedWordData> GenerateWordDataAsync(string word, CancellationToken cancellationToken = default);

    Task<string> GeneratePracticeTextAsync(PracticeTextRequest request, CancellationToken cancellationToken = default);
}
