using EnglishLearning.Application.Common.Models;

namespace EnglishLearning.Application.Common.Interfaces;

public interface IWordImportService
{
    Task<WordImportResult> ImportFromJsonAsync(Stream stream, CancellationToken cancellationToken = default);
    Task<WordImportResult> ImportFromCsvAsync(Stream stream, CancellationToken cancellationToken = default);
}
